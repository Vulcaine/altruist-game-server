using System.Net;

using Altruist;
using Altruist.Gaming.ThreeD;

using Server.Persistence;

namespace Server.GameSession;

public sealed record HttpJoinValidationResult(
    bool Success,
    string? Error,
    GameServerVault? Server,
    CharacterVault? Character,
    IGameWorldManager3D? World
);

public sealed record HandshakeValidationResult(
    bool Success,
    string? Error,
    PlayerSessionContext? Session,
    GameServerVault? Server,
    CharacterVault? Character,
    IGameWorldManager3D? World
);

internal sealed record CoreValidationResult(
    bool Success,
    string? Error,
    GameServerVault? Server,
    CharacterVault? Character,
    IGameWorldManager3D? World
);

[Service]
public sealed class GameSessionValidatorService
{
    private readonly IGameSessionService _sessionService;
    private readonly IVault<GameServerVault> _gameServerVault;
    private readonly IVault<CharacterVault> _characterVault;
    private readonly IGameWorldOrganizer3D _worldOrganizer;

    public GameSessionValidatorService(
        IGameSessionService sessionService,
        IVault<GameServerVault> gameServerVault,
        IVault<CharacterVault> characterVault,
        IGameWorldOrganizer3D worldOrganizer)
    {
        _sessionService = sessionService;
        _gameServerVault = gameServerVault;
        _characterVault = characterVault;
        _worldOrganizer = worldOrganizer;
    }

    // ------------------------------------------------------------
    // HTTP side: validate join + write PlayerSessionContext
    // ------------------------------------------------------------
    public async Task<HttpJoinValidationResult> ValidateHttpJoinAsync(
        string accountId,
        string serverId,
        string characterId)
    {
        if (string.IsNullOrWhiteSpace(accountId))
            return new HttpJoinValidationResult(false, "Missing account id.", null, null, null);

        if (string.IsNullOrWhiteSpace(serverId))
            return new HttpJoinValidationResult(false, "Missing server id.", null, null, null);

        if (string.IsNullOrWhiteSpace(characterId))
            return new HttpJoinValidationResult(false, "Missing character id.", null, null, null);

        var core = await ValidateCoreAsync(accountId, serverId, characterId);
        if (!core.Success)
            return new HttpJoinValidationResult(false, core.Error, core.Server, core.Character, core.World);

        // Persist the session intent for this account.
        var session = new PlayerSessionContext(characterId, serverId);
        await _sessionService.SetContext(accountId, session);

        return new HttpJoinValidationResult(true, null, core.Server, core.Character, core.World);
    }

    // ------------------------------------------------------------
    // Socket side: validate handshake using stored PlayerSessionContext
    // ------------------------------------------------------------
    public async Task<HandshakeValidationResult> ValidateHandshakeAsync(string accountId)
    {
        if (string.IsNullOrWhiteSpace(accountId))
            return new HandshakeValidationResult(false, "Missing account id.", null, null, null, null);

        var session = await _sessionService.GetContext<PlayerSessionContext>(accountId);
        if (session == null)
            return new HandshakeValidationResult(false, "You are not joined to any game.", null, null, null, null);

        var core = await ValidateCoreAsync(accountId, session.ServerId, session.CharacterId);
        if (!core.Success)
            return new HandshakeValidationResult(false, core.Error, session, core.Server, core.Character, core.World);

        return new HandshakeValidationResult(true, null, session, core.Server, core.Character, core.World);
    }

    // ------------------------------------------------------------
    // Core shared validation
    // ------------------------------------------------------------
    private async Task<CoreValidationResult> ValidateCoreAsync(
        string accountId,
        string serverId,
        string characterId)
    {
        var server = await _gameServerVault
            .Where(s => s.StorageId == serverId)
            .FirstOrDefaultAsync();

        if (server == null)
            return new CoreValidationResult(false, "Server not found.", null, null, null);

        if (!IsCurrentHostForServer(server))
            return new CoreValidationResult(false, "Server mismatch for this machine.", null, null, null);

        var character = await _characterVault
            .Where(c => c.StorageId == characterId && c.AccountId == accountId && c.ServerId == serverId)
            .FirstOrDefaultAsync();

        if (character == null)
            return new CoreValidationResult(false, "Character not found.", server, null, null);

        var world = _worldOrganizer.GetWorld(WorldIndicies.StartWorld);
        if (world == null)
            return new CoreValidationResult(false, "World not found.", server, character, null);

        return new CoreValidationResult(true, null, server, character, world);
    }

    // ------------------------------------------------------------
    // Helpers
    // ------------------------------------------------------------
    private static bool IsCurrentHostForServer(GameServerVault server)
    {
        var serverHost = GetHostFromServer(server);
        if (string.IsNullOrWhiteSpace(serverHost))
            return false;

        var currentHost = Dns.GetHostName();
        return string.Equals(serverHost, currentHost, StringComparison.OrdinalIgnoreCase);
    }

    private static string? GetHostFromServer(GameServerVault server)
    {
        if (string.IsNullOrWhiteSpace(server.SocketUrl))
            return null;

        if (!Uri.TryCreate(server.SocketUrl, UriKind.Absolute, out var uri))
            return null;

        return uri.Host;
    }
}

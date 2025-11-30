using Altruist;
using Altruist.Gaming.ThreeD;
using Altruist.Numerics;
using Altruist.Persistence;
using Altruist.Security;

using Server.Packet;

namespace Server.GameSession;

[SessionShield]
[Portal("/game/v1")]
public class GameSessionPortal : BaseSessionPortal
{
    private readonly IGameCharacterSessionService _characterSessionService;
    private readonly IGameWorldOrganizer3D _gameWorldOrganizer;
    private readonly ISpatialBroadcastService3D _spatialBroadcastService;

    private readonly IPrefabVault<CharacterPrefab> _characterPrefabVault;

    public GameSessionPortal(
        IGameSessionService gameSessionService,
        IGameWorldOrganizer3D gameWorldOrganizer,
        IAltruistRouter router,
        ISessionTokenValidator tokenValidator,
        IGameCharacterSessionService characterSessionService,
        ISpatialBroadcastService3D spatialBroadcastService,
        IPrefabVault<CharacterPrefab> characterPrefabVault
    ) : base(gameSessionService, router, tokenValidator)
    {
        _characterSessionService = characterSessionService;
        _gameWorldOrganizer = gameWorldOrganizer;
        _spatialBroadcastService = spatialBroadcastService;
        _characterPrefabVault = characterPrefabVault;
    }

    [Cycle("*/30 * * * * *")]
    public async Task PersistCharacterPositionsSnapshot()
    {
        foreach (var world in _gameWorldOrganizer.GetAllWorlds())
        {
            var allCharacterPrefabs = world.FindAllObjects<CharacterPrefab>();

            foreach (var characterPrefab in allCharacterPrefabs)
            {
                var characterVault = await characterPrefab.Character.LoadAsync();
                if (characterVault == null)
                    continue;

                await _characterPrefabVault.SaveAsync(characterPrefab);
            }
        }
    }

    [Cycle]
    public async Task UpdatePlayerPosition()
    {
        foreach (var world in _gameWorldOrganizer.GetAllWorlds())
        {
            var allCharacterPrefabs = world.FindAllObjects<CharacterPrefab>();

            foreach (var characterPrefab in allCharacterPrefabs)
            {
                var characterVault = await characterPrefab.Character.LoadAsync();
                if (characterVault == null)
                    continue;

                var position = characterPrefab.GetCurrentPosition();
                var (yaw, pitch, roll) = characterPrefab.GetCurrentYawPitchRoll();

                var update = new UpdateClientPositionAndOrientation(
                    position.X,
                    position.Y,
                    position.Z,
                    yaw,
                    pitch,
                    roll,
                    characterPrefab.StateFlags
                );

                var cell = new IntVector3(
                    (int)position.X,
                    (int)position.Y,
                    (int)position.Z
                );

                _ = _spatialBroadcastService.SpatialBroadcast<CharacterPrefab>(
                    world.Index.Index,
                    cell,
                    update);
            }
        }
    }

    protected override async Task<IResultPacket> OnHandshakeReceived(
        HandshakeRequestPacket message,
        string clientId,
        IResultPacket result)
    {
        var accountId = await GetAccountId(message.Token);

        var authError = EnsureAuthenticated(accountId);
        if (authError != null)
            return authError;

        var clientSession = EnsureClientSession(accountId!, clientId);
        if (clientSession == null)
            return Unauthorized("You are not joined to any game.");

        return await _characterSessionService.HandleHandshakeForSessionAsync(
            clientId,
            accountId!,
            clientSession,
            result);
    }

    private static IResultPacket Unauthorized(string message)
        => ResultPacket.Failed(TransportCode.Unauthorized, message);

    private IResultPacket? EnsureAuthenticated(string? accountId)
    {
        if (accountId == null)
            return Unauthorized("Invalid token");

        return null;
    }

    private IGameSession? EnsureClientSession(string accountId, string clientId)
    {
        var accountSession = _gameSessionService.GetSession(accountId);
        if (accountSession == null)
            return null;

        var expiresAt = DateTime.UtcNow.AddHours(GameSessionConstants.SessionExpirationHours);
        return _gameSessionService.MigrateSession(accountId, clientId, expiresAt);
    }
}

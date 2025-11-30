using Altruist;
using Altruist.Engine;
using Altruist.Security;

namespace Server.GameSession;

[JwtShield]
[Portal("/game/v1")]
public class GameSessionPortal : BaseSessionPortal
{
    private readonly IGameCharacterSessionService _characterSessionService;
    private readonly IGameSessionWorldCleanup _worldCleanup;
    private readonly IAltruistEngine _engine;

    public GameSessionPortal(
        IGameSessionService gameSessionService,
        IGameSessionWorldCleanup worldCleanup,
        IAltruistRouter router,
        IAltruistEngine altruistEngine,
        ISessionTokenValidator tokenValidator,
        IGameCharacterSessionService characterSessionService
    ) : base(gameSessionService, router, tokenValidator)
    {
        _characterSessionService = characterSessionService;
        _worldCleanup = worldCleanup;
        _engine = altruistEngine;
    }

    public override async Task OnConnectedAsync(string clientId, ConnectionManager connection)
    {
        var session = _gameSessionService.GetSession(clientId);
        if (session == null)
        {
            await _router.Client.SendAsync(
                clientId,
                ResultPacket.Failed(TransportCode.Unauthorized, "Session not found."));

            _engine.WaitForNextTick(() => connection.DisconnectAsync(clientId));
            return;
        }

        await base.OnConnectedAsync(clientId, connection);
    }

    public override async Task OnDisconnectedAsync(string clientId, Exception? exception)
    {
        await _worldCleanup.CleanupOnDisconnectAsync(clientId, exception);
        await base.OnDisconnectedAsync(clientId, exception);
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

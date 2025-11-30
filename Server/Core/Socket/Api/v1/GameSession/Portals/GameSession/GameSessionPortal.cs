using Altruist;
using Altruist.Security;
using Altruist.Web;

namespace Server.GameSession;

[JwtShield]
[Portal("/game/v1")]
public class GameSessionPortal : BaseSessionPortal
{
    private readonly IGameCharacterSessionService _characterSessionService;
    private readonly IGameSessionWorldCleanup _worldCleanup;

    public GameSessionPortal(
        IGameSessionService gameSessionService,
        IGameSessionWorldCleanup worldCleanup,
        IAltruistRouter router,
        IJwtTokenValidator tokenValidator,
        IGameCharacterSessionService characterSessionService
    ) : base(gameSessionService, router, tokenValidator)
    {
        _characterSessionService = characterSessionService;
        _worldCleanup = worldCleanup;
    }

    public override async Task OnConnectedAsync(string clientId, ConnectionManager connectionManager, AltruistConnection connection)
    {
        if (connection is not WebSocketConnection)
        {
            await DisconnectWithMessage(clientId, "Invalid connection type.", connectionManager);
            return;
        }

        var authDetails = connection.AuthDetails;

        if (authDetails == null)
        {
            await DisconnectWithMessage(clientId, "Auth details not found.", connectionManager);
            return;
        }

        EnsureClientSessionAndMigrate(authDetails.PrincipalId, clientId);

        var necessaryContexts = new[] { typeof(PlayerServerSessionContext), typeof(GameSessionContext) };
        var sessionContexts = _gameSessionService.FindContexts(clientId, necessaryContexts);

        if (sessionContexts.Count() != necessaryContexts.Count())
        {
            await DisconnectWithMessage(clientId, "Session not found.", connectionManager);
            return;
        }

        await base.OnConnectedAsync(clientId, connectionManager, connection);
    }

    private async Task DisconnectWithMessage(string clientId, string message, ConnectionManager connectionManager)
    {
        await _router.Client.SendAsync(
               clientId,
               ResultPacket.Failed(TransportCode.Unauthorized, message));
        await connectionManager.DisconnectEngineAwareAsync(clientId);
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

        var clientSession = _gameSessionService.GetSession(clientId);
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

    private IGameSession? EnsureClientSessionAndMigrate(string accountId, string clientId)
    {
        var accountSession = _gameSessionService.GetSession(accountId);
        if (accountSession == null)
            return null;

        var expiresAt = DateTime.UtcNow.AddHours(GameSessionConstants.SessionExpirationHours);
        return _gameSessionService.MigrateSession(accountId, clientId, expiresAt);
    }
}

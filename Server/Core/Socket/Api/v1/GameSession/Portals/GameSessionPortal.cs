using Altruist;
using Altruist.Security;

namespace Server.GameSession;

[SessionShield]
[Portal("/game/v1")]
public class GameSessionPortal : BaseSessionPortal
{
    private readonly IGameCharacterSessionService _characterSessionService;

    public GameSessionPortal(
        IGameSessionService gameSessionService,
        IAltruistRouter router,
        ISessionTokenValidator tokenValidator,
        IGameCharacterSessionService characterSessionService
    ) : base(gameSessionService, router, tokenValidator)
    {
        _characterSessionService = characterSessionService;
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

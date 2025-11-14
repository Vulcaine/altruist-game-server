namespace Server;

using Altruist;
using Altruist.Security;

using Server.GameSession;

[JwtShield]
[Portal("/game")]
public class ServerAuthPortal : AuthPortal
{
    private readonly IGameSessionService _gameSessionService;

    public ServerAuthPortal(IAuthService authService, IAltruistRouter router, IConnectionManager connectionManager, IGameSessionService gameSessionService)
        : base(authService, router, connectionManager)
    {
        _gameSessionService = gameSessionService;
    }

    public override async Task OnUpgradeSuccess(SessionAuthContext context, string clientId, IIssue issue)
    {
        if (issue is TokenIssue tokenIssue)
        {
            AccountSessionContext accountSessionContext = new AccountSessionContext(tokenIssue.PrincipalId);
            await _gameSessionService.SetContext(clientId, accountSessionContext);
        }

        await Task.CompletedTask;
    }
}
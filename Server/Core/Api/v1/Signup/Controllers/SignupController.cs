using Altruist.Security;
using Altruist.Security.Auth;
using Altruist.Security.Http;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Server.GameSession;

namespace Server.Signup;

[ApiController]
[Route("/api/v1/auth")]
public sealed class JwtUserSessionController : JwtAuthController
{
    private readonly IGameSessionService _gameSessionService;

    public JwtUserSessionController(
        IJwtTokenValidator jwtTokenValidator,
        ILoginService loginService,
        IAuthService authService,
        TokenSessionSyncService tokenSessionSyncService,
        IJwtTokenIssuer issuer, ILoggerFactory loggerFactory, IGameSessionService gameSessionService
    ) : base(jwtTokenValidator, loginService, tokenSessionSyncService, issuer, authService, loggerFactory)
    {
        _gameSessionService = gameSessionService;
    }

    [HttpPost("email/verify")]
    public async Task<IActionResult> Verify([FromQuery] string uid, [FromQuery] string token)
    {
        if (_loginService is IVerifyEmail loginService)
        {
            var result = await loginService.VerifyAsync(uid, token);
            return Ok(result);
        }

        return BadRequest();
    }

    protected override async Task OnUpgradeSuccess(UpgradeAuthRequest context, string clientId, IIssue issue)
    {
        if (issue is TokenIssue tokenIssue)
        {
            AccountSessionContext accountSessionContext = new AccountSessionContext(tokenIssue.PrincipalId);
            await _gameSessionService.SetContext(clientId, accountSessionContext);
        }

        await Task.CompletedTask;
    }
}

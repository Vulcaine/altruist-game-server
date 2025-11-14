using Altruist.Security;
using Altruist.Security.Auth;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Server.Signup;

[ApiController]
[Route("/api/v1/auth")]
public sealed class JwtUserSessionController : JwtAuthController
{

    private readonly IGameSessionService _gameSessionService;
    public JwtUserSessionController(
        IJwtTokenValidator jwtTokenValidator,
        ILoginService loginService,
        TokenSessionSyncService tokenSessionSyncService,
        IGameSessionService gameSessionService,
        IIssuer issuer, ILoggerFactory loggerFactory
    ) : base(jwtTokenValidator, loginService, tokenSessionSyncService, issuer, loggerFactory)
    {
        _gameSessionService = gameSessionService;
    }

    [HttpPost("verify")]
    public async Task<IActionResult> Verify([FromQuery] string uid, [FromQuery] string token)
    {
        if (_loginService is IVerifyEmail loginService)
        {
            var result = await loginService.VerifyAsync(uid, token);
            return Ok(result);
        }

        return BadRequest();
    }
}

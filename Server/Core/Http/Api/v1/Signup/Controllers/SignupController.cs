using Altruist.Security;
using Altruist.Security.Auth;
using Altruist.Security.Http;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Server.Signup;

[ApiController]
[Route("/api/v1/auth")]
public sealed class JwtUserSessionController : JwtAuthController
{
    public JwtUserSessionController(
        IJwtTokenValidator jwtTokenValidator,
        ILoginService loginService,
        IAuthService authService,
        TokenSessionSyncService tokenSessionSyncService,
        IJwtTokenIssuer issuer, ILoggerFactory loggerFactory
    ) : base(jwtTokenValidator, loginService, tokenSessionSyncService, issuer, authService, loggerFactory)
    {
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
}

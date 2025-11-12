using Altruist.Security;
using Altruist.Security.Auth;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Server.Signup;

[ApiController]
[Route("api/v1/auth/[controller]")]
public sealed class JwtUserSessionController : JwtAuthController
{
    public JwtUserSessionController(JwtTokenValidator jwtTokenValidator, ILoginService loginService, TokenSessionSyncService tokenSessionSyncService, IIssuer issuer, ILoggerFactory loggerFactory) : base(jwtTokenValidator, loginService, tokenSessionSyncService, issuer, loggerFactory)
    {
    }
}

namespace Server;

using Altruist;
using Altruist.Security;

[JwtShield]
[Portal("/game")]
public class ServerAuthPortal : AuthPortal
{
    public ServerAuthPortal(IAuthService authService) : base(authService)
    {
    }
}
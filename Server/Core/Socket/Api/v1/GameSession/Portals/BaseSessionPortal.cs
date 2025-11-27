using System.Security.Claims;

using Altruist;
using Altruist.Gaming;
using Altruist.Security;

public class BaseSessionPortal : AltruistGameSessionPortal
{
    private ISessionTokenValidator _tokenValidator;
    public BaseSessionPortal(IGameSessionService gameSessionService, IAltruistRouter router, ISessionTokenValidator tokenValidator) : base(gameSessionService, router)
    {
        _tokenValidator = tokenValidator;
    }

    public async Task<string?> GetAccountId(string token)
    {
        var validated = await _tokenValidator.ValidateToken(token);
        if (validated == null)
        {
            return null;
        }
        return validated.FindFirstValue(ClaimTypes.NameIdentifier);
    }
}

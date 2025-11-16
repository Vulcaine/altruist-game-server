

using System.Security.Claims;

using Microsoft.AspNetCore.Mvc;

public class BaseSessionController : ControllerBase
{
    public string? GetAccountId()
    {
        var principal = HttpContext.User;
        if (principal?.Identity?.IsAuthenticated != true)
            return null;

        var principalId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        return principalId;
    }
}

using System.Security.Claims;

using Altruist;
using Altruist.Security;

using Microsoft.AspNetCore.Mvc;

using Server.Persistence;

[JwtShield]
[ApiController]
[Route("/api/v1/game")]
public sealed class GameSessionController : ControllerBase
{
    private readonly IVault<Character> _characterVault;
    public GameSessionController(IVault<Character> characterVault)
    {
        _characterVault = characterVault;
    }
    [HttpGet("characters")]
    public async Task<IActionResult> GetAvailableCharacters()
    {
        var principal = HttpContext.User;
        if (principal?.Identity?.IsAuthenticated != true)
            return Unauthorized();

        var principalId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(principalId))
            return Unauthorized("Missing principal id.");

        var allCharactersForAccount = await _characterVault
            .Where(c => c.AccountId == principalId)
            .ToListAsync();
        return Ok(allCharactersForAccount);
    }
}

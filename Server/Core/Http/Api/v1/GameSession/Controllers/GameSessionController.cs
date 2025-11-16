using System.Security.Claims;

using Altruist;
using Altruist.Security;

using Microsoft.AspNetCore.Mvc;

using Server;
using Server.Persistence;

[JwtShield]
[ApiController]
[Route("/api/v1/game")]
public sealed class GameSessionController : ControllerBase
{
    private readonly IVault<Character> _characterVault;
    private readonly IGameSessionService _gameSessionService;
    public GameSessionController(
        IGameSessionService gameSessionService,
        IVault<Character> characterVault)
    {
        _gameSessionService = gameSessionService;
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

    [HttpPost("join")]
    public async Task<IActionResult> JoinGame([FromBody] JoinGameRequest request)
    {
        // _gameSessionService.JoinGameAsync
        return Ok();
    }
}

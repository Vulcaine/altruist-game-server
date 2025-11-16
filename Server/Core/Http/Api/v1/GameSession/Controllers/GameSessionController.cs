using Altruist;
using Altruist.Security;

using Microsoft.AspNetCore.Mvc;

using Server;
using Server.Persistence;

[JwtShield]
[ApiController]
[Route("/api/v1/game")]
public sealed class GameSessionController : BaseSessionController
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
        var accountId = GetAccountId();
        if (string.IsNullOrEmpty(accountId))
            return Unauthorized("Missing principal id.");

        var allCharactersForAccount = await _characterVault
            .Where(c => c.AccountId == accountId)
            .ToListAsync();
        return Ok(allCharactersForAccount);
    }

    [HttpPost("join")]
    public async Task<IActionResult> JoinGame([FromBody] JoinGameRequest request)
    {
        // TODO: get these from config
        return Ok(new JoinGameResponse("ws://localhost:8000/ws/game"));
    }
}

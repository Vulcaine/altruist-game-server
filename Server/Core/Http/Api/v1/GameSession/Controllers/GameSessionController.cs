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
    private readonly IVault<CharacterVault> _characterVault;
    private readonly IGameSessionService _gameSessionService;
    public GameSessionController(
        IGameSessionService gameSessionService,
        IVault<CharacterVault> characterVault)
    {
        _gameSessionService = gameSessionService;
        _characterVault = characterVault;
    }

    [HttpGet("characters")]
    public async Task<IActionResult> GetAvailableCharacters([FromQuery] string serverId)
    {
        var accountId = GetAccountId();
        if (string.IsNullOrEmpty(accountId))
            return Unauthorized("Missing principal id.");

        if (string.IsNullOrWhiteSpace(serverId))
            return BadRequest("Missing required query parameter 'serverId'.");

        var allCharactersForAccount = await _characterVault
            .Where(c => c.AccountId == accountId && c.ServerId == serverId)
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

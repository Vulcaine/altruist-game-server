using Altruist;
using Altruist.Security;

using Microsoft.AspNetCore.Mvc;

using Server;
using Server.GameSession;
using Server.Persistence;

[JwtShield]
[ApiController]
[Route("/api/v1/game")]
public sealed class GameSessionController : BaseSessionController
{
    private readonly IVault<CharacterVault> _characterVault;

    private readonly GameSessionValidatorService _gameSessionValidatorService;


    public GameSessionController(
        IVault<CharacterVault> characterVault,
        GameSessionValidatorService gameSessionValidatorService
    )
    {
        _characterVault = characterVault;
        _gameSessionValidatorService = gameSessionValidatorService;
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

        var summaries = allCharactersForAccount
        .Select(c => new CharacterSummary(
            id: c.StorageId,
            name: c.Name,
            properties: c.Properties ?? Array.Empty<short>(),
            world: c.World
        ))
        .ToArray();

        return Ok(summaries);
    }

    [HttpPost("join")]
    public async Task<IActionResult> JoinGame([FromBody] JoinGameRequest request)
    {
        var accountId = GetAccountId();
        if (accountId == null)
            return Unauthorized();

        var validation = await _gameSessionValidatorService.ValidateHttpJoinAsync(
                accountId,
                request.ServerId,
                request.CharacterId);

        if (!validation.Success)
            return BadRequest(validation.Error);

        return Ok(new JoinGameResponse(validation.Server!.SocketUrl));
    }
}

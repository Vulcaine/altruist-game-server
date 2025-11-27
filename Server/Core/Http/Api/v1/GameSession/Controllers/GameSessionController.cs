using Altruist;
using Altruist.Gaming.ThreeD;
using Altruist.Persistence;
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
    private readonly IPrefabVault<CharacterPrefab> _characterPrefabVault;
    private readonly IGameSessionService _gameSessionService;
    private readonly IGameWorldOrganizer3D _gameWorldOrganizer;

    public GameSessionController(
        IGameSessionService gameSessionService,
        IGameWorldOrganizer3D gameWorldCoordinator3D,
        IVault<CharacterVault> characterVault,
        IPrefabVault<CharacterPrefab> characterPrefabVault
    )
    {
        _gameSessionService = gameSessionService;
        _characterVault = characterVault;
        _gameWorldOrganizer = gameWorldCoordinator3D;
        _characterPrefabVault = characterPrefabVault;
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
        var accountId = GetAccountId();

        if (accountId == null)
        {
            return Unauthorized();
        }

        var selectedCharacter = await _characterVault
             .Where(c => c.StorageId == request.CharacterId)
             .FirstOrDefaultAsync();

        if (selectedCharacter == null)
        {
            return BadRequest("Character not found.");
        }

        var startWorld = _gameWorldOrganizer.GetWorld(WorldIndicies.StartWorld);

        if (startWorld == null)
        {
            return BadRequest("World not found.");
        }

        var characterPrefab = _characterPrefabVault.Construct();
        characterPrefab.Character.Apply(selectedCharacter);

        await startWorld.SpawnDynamicObject(characterPrefab);
        await _characterPrefabVault.SaveAsync(characterPrefab);

        var characterSession = new CharacterSessionContext(request.CharacterId);
        await _gameSessionService.SetContext(accountId, characterSession);

        return Ok(new JoinGameResponse("ws://localhost:8000/ws/game"));
    }
}

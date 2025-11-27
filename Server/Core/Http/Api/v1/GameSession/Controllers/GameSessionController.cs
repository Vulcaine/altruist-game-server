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
    private readonly IGameSessionService _gameSessionService;
    private readonly ISpawnService3D _spawnService3D;
    private readonly IPrefabFactory _prefabFactory;

    private readonly IGameWorldOrganizer3D _gameWorldOrganizer;

    public GameSessionController(
        IGameSessionService gameSessionService,
        IGameWorldOrganizer3D gameWorldCoordinator3D,
        ISpawnService3D spawnService,
        IPrefabFactory prefabFactory,
        IVault<CharacterVault> characterVault)
    {
        _gameSessionService = gameSessionService;
        _characterVault = characterVault;
        _spawnService3D = spawnService;
        _gameWorldOrganizer = gameWorldCoordinator3D;
        _prefabFactory = prefabFactory;
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

        var startWorld = _gameWorldOrganizer.GetWorld(WorldIndexDefinition.Default);

        if (startWorld == null)
        {
            return BadRequest("World not found.");
        }

        var characterPrefab = _prefabFactory.Construct<CharacterPrefab>();
        characterPrefab.Character.Apply(selectedCharacter);
        _spawnService3D.Spawn(startWorld, characterPrefab);

        var characterSession = new CharacterSessionContext(request.CharacterId);
        await _gameSessionService.SetContext(accountId, characterSession);

        return Ok(new JoinGameResponse("ws://localhost:8000/ws/game"));
    }
}

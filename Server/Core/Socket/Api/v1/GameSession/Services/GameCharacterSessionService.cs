using Altruist;
using Altruist.Persistence;

using Altruist.Gaming.Movement.ThreeD;

using Server.Persistence;
using Server.Packet;

namespace Server.GameSession;

public interface IGameCharacterSessionService
{
    Task<IResultPacket> HandleHandshakeForSessionAsync(
        string accountId,
        IGameSession clientSession,
        IResultPacket result);
}

[Service(typeof(IGameCharacterSessionService))]
public sealed class GameCharacterSessionService : IGameCharacterSessionService
{
    private readonly IPrefabVault<CharacterPrefab> _characterPrefabVault;
    private readonly GameSessionValidatorService _gameSessionValidatorService;
    private readonly IAltruistRouter _router;
    private readonly IGameMovementSessionService _movementSessionService;

    public GameCharacterSessionService(
        IPrefabVault<CharacterPrefab> characterPrefabVault,
        GameSessionValidatorService gameSessionValidatorService,
        IAltruistRouter router,
        IGameMovementSessionService movementSessionService)
    {
        _characterPrefabVault = characterPrefabVault;
        _gameSessionValidatorService = gameSessionValidatorService;
        _router = router;
        _movementSessionService = movementSessionService;
    }

    public async Task<IResultPacket> HandleHandshakeForSessionAsync(
        string accountId,
        IGameSession clientSession,
        IResultPacket result)
    {
        var validation = await _gameSessionValidatorService.ValidateHandshakeAsync(accountId);
        if (!validation.Success)
            return Unauthorized(validation.Error ?? "Handshake failed");

        // typed as dynamic in the validator, but we know these are character + world vault records
        var character = validation.Character!;
        var startWorld = validation.World!;

        await PrepareCharacterInWorldAsync(character, startWorld);
        await BroadcastCharacterJoinedAsync(character);
        await BindCharacterSessionContextAsync(clientSession, accountId, validation);

        return result;
    }

    private static IResultPacket Unauthorized(string message)
        => ResultPacket.Failed(TransportCode.Unauthorized, message);

    private async Task PrepareCharacterInWorldAsync(dynamic characterDynamic, dynamic startWorld)
    {
        var storageId = characterDynamic.StorageId;

        // Remove existing instance if already present
        var existingWorldObject = startWorld.FindObject(storageId);
        if (existingWorldObject != null)
            startWorld.DestroyObject(existingWorldObject);

        // Construct prefab and apply persisted character data
        var characterPrefab = _characterPrefabVault.Construct();
        characterPrefab.Character.Apply(characterDynamic);

        // Spawn into world and obtain PhysX body
        var body = await startWorld.SpawnDynamicObject(characterPrefab, withId: storageId);

        // Wire movement for this character based on its properties
        if (characterDynamic is CharacterBase characterBase && body is IPhysxBody3D physxBody)
        {
            _movementSessionService.SetupCharacterMovement(characterBase, characterPrefab, physxBody);
        }

        // Persist prefab state if needed
        await _characterPrefabVault.SaveAsync(characterPrefab);
    }

    private async Task BroadcastCharacterJoinedAsync(dynamic character)
    {
        var packet = new CharacterJoinedPacket
        {
            Id = character.StorageId,
            Name = character.Name,
            Properties = character.Properties
        };

        await _router.Broadcast.SendAsync(packet);
    }

    private Task BindCharacterSessionContextAsync(
        IGameSession clientSession,
        string accountId,
        dynamic validation)
    {
        var character = validation.Character!;
        var server = validation.Server!;
        var world = validation.World!;

        var context = new CharacterSessionContext(
            accountId,
            character.StorageId,
            server.StorageId,
            world.Index.Index);

        return clientSession.SetContext(character.StorageId, context);
    }
}

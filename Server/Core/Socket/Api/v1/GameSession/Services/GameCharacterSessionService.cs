using Altruist;
using Altruist.Gaming.Movement.ThreeD;
using Altruist.Gaming.ThreeD;
using Altruist.Persistence;

using Server.Packet;
using Server.Persistence;

namespace Server.GameSession;

public interface IGameCharacterSessionService
{
    Task<IResultPacket> HandleHandshakeForSessionAsync(
        string clientId,
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

    public GameCharacterSessionService(
        IPrefabVault<CharacterPrefab> characterPrefabVault,
        GameSessionValidatorService gameSessionValidatorService,
        IAltruistRouter router)
    {
        _characterPrefabVault = characterPrefabVault;
        _gameSessionValidatorService = gameSessionValidatorService;
        _router = router;
    }

    public async Task<IResultPacket> HandleHandshakeForSessionAsync(
        string clientId,
        string accountId,
        IGameSession clientSession,
        IResultPacket result)
    {
        var validation = await _gameSessionValidatorService.ValidateHandshakeAsync(clientId, accountId);
        if (!validation.Success)
            return Unauthorized(validation.Error ?? "Handshake failed");

        var character = validation.Character!;
        var startWorld = validation.World!;

        await PrepareCharacterInWorldAsync(clientId, character, startWorld);
        await BroadcastCharacterJoinedAsync(character);
        BindCharacterSessionContext(clientSession, accountId, validation);

        return result;
    }

    private static IResultPacket Unauthorized(string message)
        => ResultPacket.Failed(TransportCode.Unauthorized, message);

    private async Task PrepareCharacterInWorldAsync(
        string clientId,
        CharacterVault characterDynamic, IGameWorldManager3D startWorld)
    {
        var storageId = characterDynamic.StorageId;

        var existingWorldObject = startWorld.FindObject(storageId);
        if (existingWorldObject != null)
            startWorld.DestroyObject(existingWorldObject);

        var characterPrefab = _characterPrefabVault.Construct();
        characterPrefab.ClientId = clientId;
        characterPrefab.Character.Apply(characterDynamic);

        var body = await startWorld.SpawnDynamicObject(characterPrefab, withId: storageId);

        if (characterDynamic is CharacterBase characterBase && body is IPhysxBody3D physxBody)
        {
            characterPrefab.SetupMovement(characterBase, physxBody);
        }

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

    private void BindCharacterSessionContext(
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

        clientSession.SetContext(character.StorageId, context);
    }
}

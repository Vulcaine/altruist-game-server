using Altruist;
using Altruist.Persistence;
using Altruist.Security;

using Server.Packet;

namespace Server.GameSession;

[SessionShield]
[Portal("/game/v1")]
public class GameSessionPortal : BaseSessionPortal
{
    private readonly IPrefabVault<CharacterPrefab> _characterPrefabVault;
    private readonly GameSessionValidatorService _gameSessionValidatorService;

    public GameSessionPortal(
        IGameSessionService gameSessionService,
        IAltruistRouter router, ISessionTokenValidator tokenValidator,
        IPrefabVault<CharacterPrefab> characterPrefabVault,
        GameSessionValidatorService gameSessionValidatorService
        ) : base(gameSessionService, router, tokenValidator)
    {
        _characterPrefabVault = characterPrefabVault;
        _gameSessionValidatorService = gameSessionValidatorService;
    }

    protected override async Task<IResultPacket> OnHandshakeReceived(HandshakeRequestPacket message, string clientId, IResultPacket result)
    {
        var accountId = await GetAccountId(message.Token);
        if (accountId == null)
            return ResultPacket.Failed(TransportCode.Unauthorized, "Invalid token");

        var accountSession = _gameSessionService.GetSession(accountId);

        if (accountSession == null)
            return ResultPacket.Failed(TransportCode.Unauthorized, "You are not joined to any game.");

        var expiresAt = DateTime.UtcNow.AddHours(24);
        var clientSession = _gameSessionService.MigrateSession(accountId, clientId, expiresAt);

        if (clientSession == null)
        {
            return ResultPacket.Failed(TransportCode.Unauthorized, "You are not joined to any game.");
        }

        var validation = await _gameSessionValidatorService.ValidateHandshakeAsync(accountId);
        if (!validation.Success)
            return ResultPacket.Failed(TransportCode.Unauthorized, validation.Error ?? "Handshake failed");

        var character = validation.Character!;
        var startWorld = validation.World!;

        var existingWorldObject = startWorld.FindObject(character.StorageId);
        if (existingWorldObject != null)
            startWorld.DestroyObject(existingWorldObject);

        var characterPrefab = _characterPrefabVault.Construct();
        characterPrefab.Character.Apply(character);

        await startWorld.SpawnDynamicObject(characterPrefab, withId: character.StorageId);
        await _characterPrefabVault.SaveAsync(characterPrefab);

        var CharacterJoinedPacket = new CharacterJoinedPacket
        {
            Id = character.StorageId,
            Name = character.Name,
            Properties = character.Properties
        };

        await _router.Broadcast.SendAsync(CharacterJoinedPacket);
        await clientSession.SetContext(character.StorageId, new CharacterSessionContext(accountId, character.StorageId, validation.Server!.StorageId, validation.World!.Index.Index));
        return result;
    }


}

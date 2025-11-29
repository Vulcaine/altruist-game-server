using System.Numerics;

using Altruist;
using Altruist.Gaming.Movement.ThreeD;
using Altruist.Gaming.ThreeD;
using Altruist.Security;

using Server.Gameplay;

namespace Server.GameSession;

public class GameInputPortal : BaseSessionPortal
{
    private readonly IMovementManager3D _movementManager;
    private readonly IGameWorldOrganizer3D _gameWorldOrganizer;

    public GameInputPortal(IGameSessionService gameSessionService, IAltruistRouter router, ISessionTokenValidator tokenValidator,
    IGameWorldOrganizer3D gameWorldOrganizer, IMovementManager3D movementManager3D) : base(gameSessionService, router, tokenValidator)
    {
        _movementManager = movementManager3D;
        _gameWorldOrganizer = gameWorldOrganizer;
    }

    [Gate("input")]
    public async Task Input(InputPacket packet, string clientId)
    {
        var clientSession = _gameSessionService.GetSession(clientId);

        if (clientSession == null)
        {
            return;
        }

        var characterSession = await clientSession.GetContext<CharacterSessionContext>(clientId);

        if (characterSession == null)
        {
            return;
        }

        var playerSession = await clientSession.GetContext<PlayerSessionContext>(characterSession.AccountId);

        if (playerSession == null)
        {
            return;
        }

        var playerId = playerSession.CharacterId;
        if (!_movementManager.TryGetPlayerState(playerId, out var state))
            return;

        var characterWorld = _gameWorldOrganizer.GetWorld(characterSession.WorldIndex);

        if (characterWorld == null)
        {
            return;
        }

        var character = characterWorld.FindObject(characterSession.CharacterId);
        // TODO load character skill palette and based on that map the input intent.
        var intent = MapInputToIntent(packet, state.Orientation);
        _movementManager.SetPlayerIntent(playerId, intent);

        return;
    }

    private static MovementIntent3D MapInputToIntent(InputPacket packet, Quaternion currentOrientation)
    {
        var inputX = packet.MoveX / 100f;
        var inputY = packet.MoveY / 100f;

        var localMove = new Vector3(inputX, 0f, inputY);
        var worldMove = Vector3.Transform(localMove, currentOrientation);
        const float mouseSensitivity = 0.0015f;
        float yawDelta = packet.LookDeltaX * mouseSensitivity;
        float pitchDelta = packet.LookDeltaY * mouseSensitivity;

        bool jump = IsSlotDown(packet.Slots, InputSlots.Slot1);
        bool boost = IsSlotDown(packet.Slots, InputSlots.Slot2);
        bool dash = IsSlotDown(packet.Slots, InputSlots.Slot3);
        var aimDir = Vector3.Normalize(Vector3.Transform(Vector3.UnitZ, currentOrientation));

        return new MovementIntent3D(
            Move: worldMove,
            TurnYaw: yawDelta,
            Jump: jump,
            Boost: boost,
            Dash: dash,
            AimDirection: aimDir,
            Knockback: Vector3.Zero
        );
    }

    private static bool IsSlotDown(InputSlots slots, InputSlots flag)
        => (slots & flag) != 0;
}

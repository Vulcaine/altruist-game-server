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

    // Cache all valid slots once so we don't call Enum.GetValues on every input.
    private static readonly InputSlots[] s_allSlots = InitAllSlots();

    private static InputSlots[] InitAllSlots()
    {
        var values = (InputSlots[])Enum.GetValues(typeof(InputSlots));
        return values.Where(v => v != InputSlots.None).ToArray();
    }

    public GameInputPortal(
        IGameSessionService gameSessionService,
        IAltruistRouter router,
        ISessionTokenValidator tokenValidator,
        IGameWorldOrganizer3D gameWorldOrganizer,
        IMovementManager3D movementManager3D)
        : base(gameSessionService, router, tokenValidator)
    {
        _movementManager = movementManager3D;
        _gameWorldOrganizer = gameWorldOrganizer;
    }

    [Gate("input")]
    public async Task Input(InputPacket packet, string clientId)
    {
        var clientSession = _gameSessionService.GetSession(clientId);
        if (clientSession == null)
            return;

        var characterSession = await clientSession.GetContext<CharacterSessionContext>(clientId);
        if (characterSession == null)
            return;

        var playerSession = await clientSession.GetContext<PlayerSessionContext>(characterSession.AccountId);
        if (playerSession == null)
            return;

        var playerId = playerSession.CharacterId;
        if (!_movementManager.TryGetPlayerState(playerId, out var state))
            return;

        var characterWorld = _gameWorldOrganizer.GetWorld(characterSession.WorldIndex);
        if (characterWorld == null)
            return;

        var character = characterWorld.FindObject(characterSession.CharacterId);
        if (character is not CharacterPrefab characterPrefab)
            return;

        ISlotPalette slotPalette = await characterPrefab.LoadSlotPalette();

        var intent = MapInputToIntent(packet, state.Orientation, slotPalette);
        _movementManager.SetPlayerIntent(playerId, intent);
    }

    private MovementIntent3D MapInputToIntent(
        InputPacket packet,
        Quaternion currentOrientation,
        ISlotPalette slotPalette)
    {
        var inputX = packet.MoveX / 100f;
        var inputY = packet.MoveY / 100f;

        var localMove = new Vector3(inputX, 0f, inputY);
        var worldMove = Vector3.Transform(localMove, currentOrientation);

        const float mouseSensitivity = 0.0015f;
        float yawDelta = packet.LookDeltaX * mouseSensitivity;
        float pitchDelta = packet.LookDeltaY * mouseSensitivity;

        bool jump = IsActionDown(packet.Slots, slotPalette, IsJumpBinding);
        bool boost = IsActionDown(packet.Slots, slotPalette, IsBoostBinding);
        bool dash = IsActionDown(packet.Slots, slotPalette, IsDashBinding);

        var aimDir = Vector3.Normalize(
            Vector3.Transform(Vector3.UnitZ, currentOrientation));

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

    private static bool IsActionDown(
        InputSlots pressedSlots,
        ISlotPalette palette,
        Func<SlotBinding, bool> predicate)
    {
        foreach (var slot in s_allSlots)
        {
            if ((pressedSlots & slot) == 0)
                continue;

            var binding = palette.Get(slot);
            if (predicate(binding))
                return true;
        }

        return false;
    }

    private static bool IsJumpBinding(SlotBinding binding)
    {
        if (binding.Kind == SlotBindingKind.MovementJump)
            return true;

        return binding.Kind == SlotBindingKind.MovementAction &&
               string.Equals(binding.Id, "Jump", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsBoostBinding(SlotBinding binding)
    {
        if (binding.Kind == SlotBindingKind.MovementBoost)
            return true;

        return binding.Kind == SlotBindingKind.MovementAction &&
               string.Equals(binding.Id, "Boost", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsDashBinding(SlotBinding binding)
    {
        if (binding.Kind == SlotBindingKind.MovementDash)
            return true;

        return binding.Kind == SlotBindingKind.MovementAction &&
               string.Equals(binding.Id, "Dash", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsSlotDown(InputSlots slots, InputSlots flag)
        => (slots & flag) != 0;
}

using System.Numerics;

using Altruist;
using Altruist.Gaming;
using Altruist.Gaming.Movement.ThreeD;
using Altruist.Gaming.ThreeD;
using Altruist.Numerics;
using Altruist.Persistence;
using Altruist.ThreeD.Numerics;
using Altruist.UORM;

using Server.Data;
using Server.Gameplay;
using Server.Packet;
using Server.Persistence;

[Prefab("character", Keyspace: "player")]
[WorldObject("character")]
public class CharacterPrefab : WorldObjectPrefab3D
{
    [PrefabComponent]
    public IPrefabHandle<CharacterVault> Character { get; set; } = default!;

    [VaultIgnore]
    public ISlotPalette? Slots { get; private set; }

    [VaultIgnore]
    public CharacterStateFlags StateFlags { get; set; }

    // current movement intent from input
    private MovementIntent3D _currentIntent = MovementIntent3D.Zero;

    // movement driver is internal to the prefab
    private MovementDriver3D? _movementDriver;

    public void SetInputIntent(in MovementIntent3D intent)
    {
        _currentIntent = intent;
    }

    /// <summary>
    /// Orientation used for aiming / input mapping.
    /// Prefers movement state orientation if driver exists, otherwise uses Transform rotation.
    /// </summary>
    public Quaternion GetCurrentOrientation()
    {
        if (_movementDriver is not null)
            return _movementDriver.State.Orientation;

        return Transform.Rotation.ToQuaternion();
    }

    public (float yaw, float pitch, float roll) GetCurrentYawPitchRoll() => ToYawPitchRoll(GetCurrentOrientation());

    public Vector3 GetCurrentPosition()
    {
        if (_movementDriver is not null)
            return _movementDriver.State.Position;

        return Transform.Position.ToVector3().ToFloatVector3();
    }

    [PostConstruct]
    public void Init()
    {
        const float radius = 0.5f;
        const float halfLength = 1.0f;

        var bodyProfile = new HumanoidCapsuleBodyProfile(radius, halfLength, 75f);
        BodyDescriptor = bodyProfile.CreateBody(Transform);
    }

    [OnPrefabComponentLoad(nameof(Character))]
    public async Task OnCharacterLoaded(
        CharacterVault character,
        IVault<SlotPaletteVault> slotPaletteVault)
    {
        var rows = await slotPaletteVault
            .Where(p => p.CharacterId == character.StorageId)
            .ToListAsync();

        var palette = new SlotPalette();

        foreach (var row in rows)
        {
            var slot = SlotIndexMapper.ToInputSlot(row.SlotIndex);
            if (slot == InputSlots.None)
                continue;

            var binding = new SlotBinding(row.Kind, row.BindingId);
            palette.Set(slot, binding);
        }

        Slots = palette;
    }

    public void SetupMovement(CharacterBase character, IPhysxBody3D body)
    {
        if (character is null)
            throw new ArgumentNullException(nameof(character));
        if (body is null)
            throw new ArgumentNullException(nameof(body));

        var profile = BuildMovementProfileFromCharacter(character);

        var pipeline = new MovementBuilder3D()
            .WithKinematics(Planar3DFlags.GroundPlane)
            .WithRotation(Rotation3DFlags.YawPitchRollRate | Rotation3DFlags.FaceVelocity)
            .WithDynamics(Dynamics3DFlags.LinearAccel | Dynamics3DFlags.ExponentialDrag)
            .WithForces(Forces3DFlags.Boost | Forces3DFlags.Dash | Forces3DFlags.Knockback)
            .WithConstraints(p =>
            {
                p.MaxSpeed = profile.MaxSpeed;
                p.Acceleration = profile.Acceleration;
            })
            .Build();

        IntVector3 positionVector = Transform.Position.ToVector3();
        var orientation = Transform.Rotation.ToQuaternion();

        var initialState = new MovementState3D(
            Body: body,
            Position: positionVector.ToFloatVector3(),
            Velocity: Vector3.Zero,
            Orientation: orientation
        );

        var movementEngine = Dependencies.Inject<IPhysxMovementEngine3D>();

        _movementDriver = new MovementDriver3D(
            body,
            profile,
            initialState,
            pipeline,
            movementEngine);
    }

    private MovementProfile3D BuildMovementProfileFromCharacter(CharacterBase character)
    {
        short movementSpeedRaw = character.GetProperty(CharacterProperty.MovementSpeed);
        short accelerationRaw = character.GetProperty(CharacterProperty.Acceleration);

        float maxSpeed = Math.Max(1f, movementSpeedRaw);
        float accel = Math.Max(0.1f, accelerationRaw > 0 ? accelerationRaw : maxSpeed * 2f);

        return new MovementProfile3D
        {
            MaxSpeed = maxSpeed,
            Acceleration = accel
        };
    }

    public override async Task Step(float dt, IWorldPhysics3D physics)
    {
        await base.Step(dt, physics);

        if (_movementDriver is null || Body is null)
            return;

        _movementDriver.Step(in _currentIntent, dt);

        var state = _movementDriver.State;
        Transform = Transform
            .WithPosition(Position3D.From(state.Position))
            .WithRotation(Rotation3D.FromQuaternion(state.Orientation));

        var characterVault = await Character.LoadAsync();

        if (characterVault == null)
        {
            return;
        }

        characterVault.X = (int)state.Position.X;
        characterVault.Y = (int)state.Position.Y;
        characterVault.Z = (int)state.Position.Z;

        var (yaw, pitch, roll) = GetCurrentYawPitchRoll();
        characterVault.Yaw = yaw;
        characterVault.Pitch = pitch;
        characterVault.Roll = roll;
    }

    private static (float Yaw, float Pitch, float Roll) ToYawPitchRoll(Quaternion q)
    {
        const float epsilon = 1e-6f;

        // Handle zero quaternion: treat as identity (no rotation)
        float lenSq = q.LengthSquared();
        if (lenSq < epsilon)
        {
            // You can change this to whatever default you want,
            // but zero yaw/pitch/roll is usually safe.
            return (0f, 0f, 0f);
        }

        // Normalize *safely* if not already unit
        if (MathF.Abs(lenSq - 1f) > epsilon)
        {
            float invLen = 1f / MathF.Sqrt(lenSq);
            q.X *= invLen;
            q.Y *= invLen;
            q.Z *= invLen;
            q.W *= invLen;
        }

        // Yaw (Y axis)
        float siny_cosp = 2f * (q.W * q.Y + q.Z * q.X);
        float cosy_cosp = 1f - 2f * (q.Y * q.Y + q.Z * q.Z);
        float yaw = MathF.Atan2(siny_cosp, cosy_cosp);

        // Pitch (X axis)
        float sinp = 2f * (q.W * q.X - q.Z * q.Y);

        // Clamp sinp to [-1, 1] to avoid NaN from Asin due to FP error
        if (sinp > 1f)
            sinp = 1f;
        else if (sinp < -1f)
            sinp = -1f;

        float pitch = MathF.Asin(sinp);

        // Roll (Z axis)
        float sinr_cosp = 2f * (q.W * q.Z + q.X * q.Y);
        float cosr_cosp = 1f - 2f * (q.Z * q.Z + q.X * q.X);
        float roll = MathF.Atan2(sinr_cosp, cosr_cosp);

        return (yaw, pitch, roll);
    }

    public async Task<ISlotPalette> LoadSlotPalette()
    {
        await Character.LoadAsync();
        return Slots!;
    }
}

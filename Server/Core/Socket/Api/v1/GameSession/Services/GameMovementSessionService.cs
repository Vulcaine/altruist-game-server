using System.Numerics;

using Altruist;
using Altruist.Gaming.Movement.ThreeD;

using Server.Data;
using Server.Persistence;

namespace Server.GameSession;

public interface IGameMovementSessionService
{
    void SetupCharacterMovement(CharacterBase character, CharacterPrefab characterPrefab, IPhysxBody3D body);
    MovementProfile3D BuildMovementProfileFromCharacter(CharacterBase character);

    /// <summary>
    /// Try to get the current movement state for the given player id.
    /// </summary>
    bool TryGetPlayerState(string playerId, out MovementState3D state);
}

[Service(typeof(IGameMovementSessionService))]
public sealed class GameMovementSessionService : IGameMovementSessionService
{
    private readonly IMovementManager3D _movementManager3D;

    public GameMovementSessionService(IMovementManager3D movementManager3D)
    {
        _movementManager3D = movementManager3D;
    }

    public void SetupCharacterMovement(CharacterBase character, CharacterPrefab characterPrefab, IPhysxBody3D body)
    {
        if (character == null)
            throw new ArgumentNullException(nameof(character));
        if (characterPrefab == null)
            throw new ArgumentNullException(nameof(characterPrefab));
        if (body == null)
            throw new ArgumentNullException(nameof(body));

        var playerId = character.StorageId;

        // Avoid double registration
        if (_movementManager3D.TryGetPlayerState(playerId, out _))
            return;

        var profile = BuildMovementProfileFromCharacter(character);

        var pipeline = new MovementBuilder3D()
            .WithKinematics(Planar3D.GroundPlane)
            .WithRotation(Rotation3D.YawPitchRollRate | Rotation3D.FaceVelocity)
            .WithDynamics(Dynamics3D.LinearAccel | Dynamics3D.ExponentialDrag)
            .WithForces(Forces3D.Boost | Forces3D.Dash | Forces3D.Knockback)
            .WithConstraints(p =>
            {
                p.MaxSpeed = profile.MaxSpeed;
                p.Acceleration = profile.Acceleration;
                // Add more constraint wiring (friction, jump, etc.) as needed
            })
            .Build();

        var positionVector = characterPrefab.Transform.Position.ToVector3();
        var orientation = characterPrefab.Transform.Rotation.ToQuaternion();

        var initialState = new MovementState3D(
            Body: body,
            Position: new Vector3(positionVector.X, positionVector.Y, positionVector.Z),
            Velocity: Vector3.Zero,
            Orientation: orientation
        );

        _movementManager3D.AddPlayer(
            playerId,
            body,
            profile,
            initialState,
            pipeline);
    }

    public MovementProfile3D BuildMovementProfileFromCharacter(CharacterBase character)
    {
        if (character == null)
            throw new ArgumentNullException(nameof(character));

        short movementSpeedRaw = character.GetProperty(CharacterProperty.MovementSpeed);
        short accelerationRaw = character.GetProperty(CharacterProperty.Acceleration);

        float maxSpeed = Math.Max(1f, movementSpeedRaw);
        float accel = Math.Max(0.1f, accelerationRaw > 0 ? accelerationRaw : maxSpeed * 2f);

        var profile = new MovementProfile3D
        {
            MaxSpeed = maxSpeed,
            Acceleration = accel,
        };

        return profile;
    }

    public bool TryGetPlayerState(string playerId, out MovementState3D state)
    {
        return _movementManager3D.TryGetPlayerState(playerId, out state);
    }
}

using System.Numerics;

using Altruist.Gaming.ThreeD;
using Altruist.Numerics;
using Altruist.Physx.Contracts;
using Altruist.Physx.ThreeD;
using Altruist.ThreeD.Numerics;

namespace Server.Gameplay;

/// <summary>
/// Runtime world object for a projectile spawned from a ProjectileSpell.
/// </summary>
public sealed class ProjectileObject3D : WorldObject3D
{
    public ProjectileSpell Spell { get; }
    public string OwnerId { get; }
    public Vector3 Direction { get; private set; }

    /// <summary>Total distance traveled so far.</summary>
    public float TraveledDistance { get; private set; }

    /// <summary>Whether this projectile has finished (hit something or expired).</summary>
    public bool IsExpired { get; private set; }

    public ProjectileObject3D(
        Transform3D transform,
        ProjectileSpell spell,
        IntVector3 origin,
        Vector3 direction,
        string ownerId,
        string zoneId = "") : base(transform, zoneId)
    {
        Spell = spell;
        OwnerId = ownerId;

        if (direction == Vector3.Zero)
            direction = Vector3.UnitZ;
        Direction = Vector3.Normalize(direction);

        Transform = Transform3D.From(origin, Quaternion.Identity, new Vector3(
                    spell.CollisionRadius,
                    spell.CollisionRadius,
                    spell.CollisionRadius));

        BodyDescriptor = PhysxBody3D.Create(
            type: PhysxBodyType.Dynamic,
            mass: 1f,
            transform: Transform);
    }

    public override Task Step(float dt, IWorldPhysics3D physics)
    {
        var desiredVelocity = Direction * Spell.ProjectileSpeed;
        physics.Motion.SetLinearVelocity(this, desiredVelocity);

        TraveledDistance += Spell.ProjectileSpeed * dt;
        if (TraveledDistance >= Spell.MaxTravelDistance)
        {
            Expired = true;
            return Task.CompletedTask;
        }

        return Task.CompletedTask;
    }
}

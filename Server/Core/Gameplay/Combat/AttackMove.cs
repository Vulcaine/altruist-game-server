
namespace Server.Gameplay;


public abstract class AttackMove
{
    public string Name { get; }
    public float CooldownSeconds { get; }
    public float ManaCost { get; }
    public float StaminaCost { get; }
    public float BaseDamage { get; }

    protected AttackMove(
        string name,
        float cooldownSeconds,
        float manaCost,
        float staminaCost,
        float baseDamage)
    {
        Name = name;
        CooldownSeconds = cooldownSeconds;
        ManaCost = manaCost;
        StaminaCost = staminaCost;
        BaseDamage = baseDamage;
    }
}

/// <summary>
/// Melee / simple single-target instant attack.
/// </summary>
public class BasicAttack : AttackMove
{
    public float Range { get; }

    public BasicAttack(
        string name,
        float baseDamage,
        float range,
        float cooldownSeconds,
        float manaCost = 0f,
        float staminaCost = 0f)
        : base(name, cooldownSeconds, manaCost, staminaCost, baseDamage)
    {
        Range = range;
    }
}

/// <summary>
/// Simple targeted spell (no projectile, no AoE).
/// </summary>
public class Spell : AttackMove
{
    public float Range { get; }
    public float CastTimeSeconds { get; }
    public bool RequiresTarget { get; }

    public Spell(
        string name,
        float baseDamage,
        float range,
        float cooldownSeconds,
        float castTimeSeconds = 0f,
        float manaCost = 0f,
        float staminaCost = 0f,
        bool requiresTarget = true)
        : base(name, cooldownSeconds, manaCost, staminaCost, baseDamage)
    {
        Range = range;
        CastTimeSeconds = castTimeSeconds;
        RequiresTarget = requiresTarget;
    }
}

/// <summary>
/// Spell that travels as a projectile (can be single-target or explode).
/// </summary>
public sealed class ProjectileSpell : Spell
{
    public float ProjectileSpeed { get; }
    public float MaxTravelDistance { get; }
    public float ExplosionRadius { get; }
    public float CollisionRadius { get; }

    public bool HasExplosion => ExplosionRadius > 0f;

    public ProjectileSpell(
        string name,
        float baseDamage,
        float range,
        float cooldownSeconds,
        float projectileSpeed,
        float castTimeSeconds = 0f,
        float explosionRadius = 0f,
        float collisionRadius = 0.25f,
        float manaCost = 0f,
        float staminaCost = 0f,
        bool requiresTarget = false)
        : base(name, baseDamage, range, cooldownSeconds, castTimeSeconds, manaCost, staminaCost, requiresTarget)
    {
        ProjectileSpeed = projectileSpeed;
        MaxTravelDistance = range;
        ExplosionRadius = explosionRadius;
        CollisionRadius = collisionRadius;
    }
}


/// <summary>
/// Area spell: ground AoE, nova around caster, cone, etc.
/// </summary>
public sealed class AoESpell : Spell
{
    public float AreaRadius { get; }
    public float ConeAngleDegrees { get; }
    public float DurationSeconds { get; }
    public float TickIntervalSeconds { get; }
    public float ChannelDurationSeconds { get; }

    public bool IsCone => ConeAngleDegrees > 0f;
    public bool IsPersistent => DurationSeconds > 0f && TickIntervalSeconds > 0f;
    public bool IsChanneled => ChannelDurationSeconds > 0f;

    public AoESpell(
        string name,
        float baseDamage,
        float range,
        float cooldownSeconds,
        float areaRadius,
        float coneAngleDegrees = 0f,
        float durationSeconds = 0f,
        float tickIntervalSeconds = 0f,
        float channelDurationSeconds = 0f,
        float castTimeSeconds = 0f,
        float manaCost = 0f,
        float staminaCost = 0f,
        bool requiresTarget = false)
        : base(name, baseDamage, range, cooldownSeconds, castTimeSeconds, manaCost, staminaCost, requiresTarget)
    {
        AreaRadius = areaRadius;
        ConeAngleDegrees = coneAngleDegrees;
        DurationSeconds = durationSeconds;
        TickIntervalSeconds = tickIntervalSeconds;
        ChannelDurationSeconds = channelDurationSeconds;
    }
}

/// <summary>
/// Example of a separate kind of melee that can chain/combo.
/// (You can expand with extra fields as needed.)
/// </summary>
public sealed class ComboAttack : BasicAttack
{
    public int Steps { get; }

    public ComboAttack(
        string name,
        float baseDamage,
        float range,
        float cooldownSeconds,
        int steps,
        float manaCost = 0f,
        float staminaCost = 0f)
        : base(name, baseDamage, range, cooldownSeconds, manaCost, staminaCost)
    {
        Steps = steps;
    }
}

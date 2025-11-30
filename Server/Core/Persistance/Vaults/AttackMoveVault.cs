using Altruist;
using Altruist.UORM;

namespace Server.Persistence;

[Vault("attack-move")]
public class AttackMoveVault : VaultModel
{
    [VaultColumn("name")]
    public string Name { get; set; } = string.Empty;

    [VaultColumn("cooldown-seconds")]
    public float CooldownSeconds { get; set; } = 0f;

    [VaultColumn("mana-cost")]
    public float ManaCost { get; set; } = 0f;

    [VaultColumn("stamina-cost")]
    public float StaminaCost { get; set; } = 0f;

    // Targeting / space
    /// <summary>Max usable distance. 0 = self only.</summary>
    [VaultColumn("range")]
    public float Range { get; set; } = 0f;

    /// <summary>Area radius. 0 = single-target / ray.</summary>
    [VaultColumn("area-radius")]
    public float AreaRadius { get; set; } = 0f;

    /// <summary>
    /// Optional cone angle. If > 0 and AreaRadius > 0, treat as cone instead of circle.
    /// </summary>
    [VaultColumn("cone-angle-degrees")]
    public float ConeAngleDegrees { get; set; } = 0f;

    /// <summary>Projectile speed. 0 = no projectile (instant / melee / ray).</summary>
    [VaultColumn("projectile-speed")]
    public float ProjectileSpeed { get; set; } = 0f;

    // Timing
    /// <summary>Cast time. 0 = instant.</summary>
    [VaultColumn("cast-time-seconds")]
    public float CastTimeSeconds { get; set; } = 0f;

    /// <summary>
    /// Duration of the spawned effect (buff, aura, ground AoE, etc).
    /// 0 = no persistent object.
    /// </summary>
    [VaultColumn("duration-seconds")]
    public float DurationSeconds { get; set; } = 0f;

    /// <summary>
    /// Channel duration. 0 = not channeled.
    /// If > 0, character is considered "channeling" until this elapses.
    /// </summary>
    [VaultColumn("channel-duration-seconds")]
    public float ChannelDurationSeconds { get; set; } = 0f;

    /// <summary>
    /// Interval between periodic ticks (damage/heal).
    /// 0 = no periodic ticks.
    /// </summary>
    [VaultColumn("tick-interval-seconds")]
    public float TickIntervalSeconds { get; set; } = 0f;

    [VaultColumn("requires-target")]
    public bool RequiresTarget { get; set; } = true;

    [VaultColumn("base-damage")]
    public float BaseDamage { get; set; } = 0f;
}

using Altruist;
using Altruist.UORM;

namespace Server.Persistence;

[Vault("attack-move", Keyspace: "gameplay")]
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

    // ---------- Combo windows ----------

    /// <summary>
    /// When (relative seconds from animation start) input is allowed to chain
    /// into the next move. If Start == End == 0, treat as "no chaining".
    /// </summary>
    [VaultColumn("input-window-start-seconds")]
    public float InputWindowStartSeconds { get; set; } = 0f;

    [VaultColumn("input-window-end-seconds")]
    public float InputWindowEndSeconds { get; set; } = 0f;

    /// <summary>
    /// When the move can be cancelled into something else (roll, block, etc).
    /// </summary>
    [VaultColumn("cancel-window-start-seconds")]
    public float CancelWindowStartSeconds { get; set; } = 0f;

    [VaultColumn("cancel-window-end-seconds")]
    public float CancelWindowEndSeconds { get; set; } = 0f;

    /// <summary>
    /// When damage is applied (e.g. swing impact frame).
    /// </summary>
    [VaultColumn("damage-window-start-seconds")]
    public float DamageWindowStartSeconds { get; set; } = 0f;

    [VaultColumn("damage-window-end-seconds")]
    public float DamageWindowEndSeconds { get; set; } = 0f;
}

[Vault("attack-input-binding", Keyspace: "gameplay")]
public class AttackInputBindingVault : VaultModel
{
    /// <summary>
    /// Logical combo profile this binding belongs to (optional, but useful if you
    /// want different bindings per weapon/class). If you don't need profiles yet,
    /// just keep one row per input globally.
    /// </summary>
    [VaultColumn("combo-profile-id", nullable: true)]
    [VaultForeignKey(typeof(AttackComboProfileVault), nameof(StorageId))]
    public string? ComboProfileId { get; set; }

    /// <summary>
    /// Representing the key combination (e.g. Ctrl+E).
    /// Backed by int/smallint in PostgreSQL.
    /// </summary>
    [VaultColumn("input-description")]
    public string InputDescription { get; set; } = "";

    /// <summary>
    /// Which move this input should trigger.
    /// </summary>
    [VaultColumn("attack-move-id")]
    [VaultForeignKey(typeof(AttackMoveVault), nameof(StorageId))]
    public string AttackMoveId { get; set; } = string.Empty;
}

[Vault("attack-combo-transition", Keyspace: "gameplay")]
public class AttackComboTransitionVault : VaultModel
{
    /// <summary>
    /// Optional combo profile this transition belongs to.
    /// </summary>
    [VaultColumn("combo-profile-id", nullable: true)]
    [VaultForeignKey(typeof(AttackComboProfileVault), nameof(StorageId))]
    public string? ComboProfileId { get; set; }

    /// <summary>
    /// Move we are transitioning FROM.
    /// Nullable to support "from idle / entry" if you want.
    /// </summary>
    [VaultColumn("from-attack-move-id", nullable: true)]
    [VaultForeignKey(typeof(AttackMoveVault), nameof(StorageId))]
    public string? FromAttackMoveId { get; set; }

    /// <summary>
    /// Move we are transitioning TO.
    /// </summary>
    [VaultColumn("to-attack-move-id")]
    [VaultForeignKey(typeof(AttackMoveVault), nameof(StorageId))]
    public string ToAttackMoveId { get; set; } = string.Empty;

    /// <summary>
    /// Optional priority if multiple transitions compete for the same input.
    /// 0 = default.
    /// </summary>
    [VaultColumn("priority")]
    public int Priority { get; set; } = 0;
}

[Vault("attack-combo-profile", Keyspace: "gameplay")]
[VaultUniqueKey("name")]
public class AttackComboProfileVault : VaultModel
{
    [VaultColumn("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Optional description / notes (designer friendly).
    /// </summary>
    [VaultColumn("description", nullable: true)]
    public string? Description { get; set; }

    /// <summary>
    /// The default "entry" move for this combo graph.
    /// Can be null if you want a purely input-driven start.
    /// </summary>
    [VaultColumn("entry-attack-move-id", nullable: true)]
    [VaultForeignKey(typeof(AttackMoveVault), nameof(StorageId))]
    public string? EntryAttackMoveId { get; set; }

    /// <summary>
    /// After this many seconds of no combo input, reset to entry / idle.
    /// 0 or negative = never auto reset.
    /// </summary>
    [VaultColumn("reset-to-entry-after-seconds")]
    public float ResetToEntryAfterSeconds { get; set; } = 0f;
}
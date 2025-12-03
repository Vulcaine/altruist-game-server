using Altruist;
using Altruist.UORM;

using Server.Data;

namespace Server.Persistence;

[Vault("effect", Keyspace: "gameplay")]
public class EffectVault : VaultModel
{
    [VaultColumn("effect-id")]
    public string EffectId { get; set; } = Guid.NewGuid().ToString();

    [VaultColumn("effect-applier")]
    [VaultForeignKey(typeof(CharacterVault), nameof(StorageId))]
    public string EffectApplier { get; set; } = string.Empty;

    // The effect class which drives the computation
    [VaultColumn("driver-class", nullable: true)]
    public string? EffectDriverClass { get; set; }

    [VaultColumn("effect-type")]
    public EffectTypes EffectType { get; set; }

    [VaultColumn("effect-value")]
    public float EffectValue { get; set; }

    [VaultColumn("character-id")]
    [VaultForeignKey(typeof(CharacterVault), nameof(StorageId))]
    public string CharacterId { get; set; } = string.Empty;

    [VaultColumn("started-at")]
    public DateTime StartedAt { get; set; }

    [VaultColumn("expires-at")]
    public DateTime ExpiresAt { get; set; }

    // Whether the effect is active (used for soft-expire or temporary pause)
    [VaultColumn("active")]
    public bool Active { get; set; } = true;
}

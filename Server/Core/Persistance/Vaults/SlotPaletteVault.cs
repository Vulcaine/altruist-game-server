using Altruist;
using Altruist.UORM;

using Server.Gameplay;

namespace Server.Persistence;

[Vault("slot-palette", Keyspace: "player")]
public class SlotPaletteVault : VaultModel
{
    [VaultColumn("character")]
    [VaultForeignKey(typeof(CharacterVault), nameof(StorageId))]
    public string CharacterId { get; set; } = "";

    [VaultColumn("slot-index")]
    public int SlotIndex { get; set; }

    [VaultColumn("kind")]
    public SlotBindingKind Kind { get; set; }

    [VaultColumn("binding-id")]
    public string BindingId { get; set; } = "";
}

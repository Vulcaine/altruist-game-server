using Altruist;
using Altruist.UORM;

using Server.Gameplay;

namespace Server.Persistence;

[Vault("slot_palette")]
public class SlotPaletteVault : VaultModel
{
    [VaultColumn("character")]
    [VaultForeignKey(typeof(CharacterVault), nameof(StorageId))]
    public string CharacterId { get; set; } = "";

    [VaultColumn("slot_index")]
    public int SlotIndex { get; set; }

    [VaultColumn("kind")]
    public SlotBindingKind Kind { get; set; }

    [VaultColumn("binding_id")]
    public string BindingId { get; set; } = "";
}

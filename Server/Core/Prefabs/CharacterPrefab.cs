using Altruist.Gaming;
using Altruist.Gaming.ThreeD;

using Server.Persistence;
using Altruist.Persistence;
using Altruist;
using Server.Gameplay;
using Server.Packet;

[Prefab("character")]
[WorldObject("character")]
public class CharacterPrefab : WorldObjectPrefab3D
{
    [PrefabComponent]
    public IPrefabHandle<CharacterVault> Character { get; set; } = default!;

    public string ClientId { get; set; } = "";

    public ISlotPalette? Slots { get; private set; }

    public CharacterStateFlags StateFlags { get; set; }

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

    public async Task<ISlotPalette> LoadSlotPalette()
    {
        await Character.LoadAsync();
        return Slots!;
    }
}

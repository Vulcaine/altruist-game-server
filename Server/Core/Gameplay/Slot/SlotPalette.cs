namespace Server.Gameplay;

[Flags]
public enum InputSlots : uint
{
    None = 0,

    Slot1 = 1u << 5,
    Slot2 = 1u << 6,
    Slot3 = 1u << 7,
    Slot4 = 1u << 8,
    Slot5 = 1u << 9,
    Slot6 = 1u << 10,
    Slot7 = 1u << 11,
    Slot8 = 1u << 12,
    Slot9 = 1u << 13,
    Slot10 = 1u << 14,
    Slot11 = 1u << 15,
    Slot12 = 1u << 16,
}

public static class SlotIndexMapper
{
    public static InputSlots ToInputSlot(int index) => index switch
    {
        1 => InputSlots.Slot1,
        2 => InputSlots.Slot2,
        3 => InputSlots.Slot3,
        4 => InputSlots.Slot4,
        5 => InputSlots.Slot5,
        6 => InputSlots.Slot6,
        7 => InputSlots.Slot7,
        8 => InputSlots.Slot8,
        9 => InputSlots.Slot9,
        10 => InputSlots.Slot10,
        11 => InputSlots.Slot11,
        12 => InputSlots.Slot12,
        _ => InputSlots.None
    };

    public static int ToIndex(InputSlots slot) => slot switch
    {
        InputSlots.Slot1 => 1,
        InputSlots.Slot2 => 2,
        InputSlots.Slot3 => 3,
        InputSlots.Slot4 => 4,
        InputSlots.Slot5 => 5,
        InputSlots.Slot6 => 6,
        InputSlots.Slot7 => 7,
        InputSlots.Slot8 => 8,
        InputSlots.Slot9 => 9,
        InputSlots.Slot10 => 10,
        InputSlots.Slot11 => 11,
        InputSlots.Slot12 => 12,
        _ => 0
    };
}

public interface ISlotPalette
{
    SlotBinding Get(InputSlots slot);
    void Set(InputSlots slot, SlotBinding binding);
}

public enum SlotBindingKind
{
    None,
    Spell,
    Item,
    MovementAction
}

public readonly record struct SlotBinding(
      SlotBindingKind Kind,
      string Id // e.g. "Jump", "Spell_Fireball", "Item_HealthPotion"
);

/// <summary>
/// Simple in-memory implementation of ISlotPalette.
/// Backed by a dictionary of InputSlots -> SlotBinding.
/// </summary>
public sealed class SlotPalette : ISlotPalette
{
    private readonly Dictionary<InputSlots, SlotBinding> _bindings =
        new Dictionary<InputSlots, SlotBinding>();

    public SlotPalette()
    {
    }

    public SlotPalette(IEnumerable<(InputSlots Slot, SlotBinding Binding)> initialBindings)
    {
        foreach (var (slot, binding) in initialBindings)
        {
            if (binding.Kind != SlotBindingKind.None)
                _bindings[slot] = binding;
        }
    }

    public SlotBinding Get(InputSlots slot)
    {
        return _bindings.TryGetValue(slot, out var binding)
            ? binding
            : default;
    }

    public void Set(InputSlots slot, SlotBinding binding)
    {
        if (binding.Kind == SlotBindingKind.None)
        {
            _bindings.Remove(slot);
        }
        else
        {
            _bindings[slot] = binding;
        }
    }

    public IEnumerable<(InputSlots Slot, SlotBinding Binding)> GetAll()
        => _bindings.Select(kv => (kv.Key, kv.Value));
}

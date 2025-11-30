namespace Server.Data;

/// <summary>
/// Property indices for Character.Properties (short[]).
/// Add new properties here; keep values stable once in production.
/// </summary>
public enum CharacterProperty : int
{
    None,
    Level,
    Experience,
    // Stats:
    Health,
    MaxHealth,
    Mana,
    MaxMana,
    Stamina,
    MaxStamina,
    Strength,
    Agility,
    Intelligence,
    Vitality,
    // we dont want CriticalChance, we will have moves
    // that does critical damage 100%, so we only need crit damage
    CriticalDamage,
    AttackSpeed,
    MovementSpeed,
    Acceleration,
    Friction,
    Max,
}


public enum CharacterStates
{
    None,
    Stunned,
    Broken,
    Poisoned,
    Dead
}


public enum EffectTypes
{
    None,
    Defense,
    Poison
}

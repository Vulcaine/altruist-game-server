namespace Server.Data;

/// <summary>
/// Property indices for Character.Properties (short[]).
/// Add new properties here; keep values stable once in production.
/// </summary>
public enum CharacterProperty : int
{
    Level = 0,
    MovementSpeed = 1,
    Acceleration = 2,
    Friction = 3,
    Max = 100,
    // Future:
    // Health = 1,
    // Mana = 2,
    // Strength = 3,
    // etc...
}

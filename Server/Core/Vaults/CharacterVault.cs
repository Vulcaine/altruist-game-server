using Altruist;
using Altruist.UORM;

using Server.Data;

namespace Server.Persistence;

[Vault("character")]
public class Character : VaultModel
{
    [VaultColumn("accountId")]
    public string AccountId { get; set; } = string.Empty;

    [VaultColumn("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Packed properties indexed by CharacterProperty enum.
    /// Stored as smallint[] in PostgreSQL.
    /// </summary>
    [VaultColumn("properties")]
    public short[] Properties { get; set; } = Array.Empty<short>();

    /// <summary>
    /// Current world / shard / zone the character is in.
    /// </summary>
    [VaultColumn("world")]
    public string World { get; set; } = string.Empty;

    public short GetProperty(CharacterProperty property)
    {
        var index = (int)property;
        if (Properties == null || index < 0 || index >= Properties.Length)
            return 0;
        return Properties[index];
    }

    public void SetProperty(CharacterProperty property, short value)
    {
        var index = (int)property;
        if (index < 0)
            throw new ArgumentOutOfRangeException(nameof(property));

        if (Properties == null || Properties.Length <= index)
        {
            var newLength = Math.Max(index + 1, Properties?.Length ?? 0);
            var newArray = new short[newLength];
            if (Properties != null)
                Array.Copy(Properties, newArray, Properties.Length);
            Properties = newArray;
        }

        Properties[index] = value;
    }
}

using Altruist;
using Altruist.UORM;

using Server.Data;

namespace Server.Persistence;

public abstract class CharacterBase : VaultModel
{
    [VaultColumn("account-id")]
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

    [VaultColumn("x")]
    public int X { get; set; }

    [VaultColumn("y")]
    public int Y { get; set; }

    [VaultColumn("z")]
    public int Z { get; set; }

    [VaultColumn("yaw")]
    public float Yaw { get; set; }

    [VaultColumn("pitch")]
    public float Pitch { get; set; }

    [VaultColumn("roll")]
    public float Roll { get; set; }

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

[Vault("character-template")]
public class CharacterTemplateVault : CharacterBase
{
    [VaultColumn("template-code")]
    [VaultUniqueColumn]
    public string TemplateCode { get; set; } = string.Empty;
}

[Vault("character")]
public class CharacterVault : CharacterBase
{
    [VaultColumn("template-code")]
    [VaultForeignKey(typeof(CharacterTemplateVault), nameof(CharacterTemplateVault.TemplateCode))]
    public string TemplateCode { get; set; } = string.Empty;

    [VaultColumn("server-id")]
    [VaultForeignKey(typeof(GameServerVault), nameof(StorageId))]
    public string ServerId { get; set; } = string.Empty;

    [VaultColumn("world-id")]
    public int WorldIndex { get; set; } = WorldIndicies.StartWorld;
}

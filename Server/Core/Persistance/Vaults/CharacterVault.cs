
using System.Text.Json;

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
    [VaultColumn("world-index")]
    public int WorldIndex { get; set; } = WorldIndicies.StartWorld;

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

    public Task<List<CharacterTemplateVault>> OnCreateAsync(IServiceProvider serviceProvider)
    {
        var adminCharacter = new CharacterTemplateVault()
        {
            TemplateCode = "character",
            Name = "Admin",
            WorldIndex = 0
        };
        return Task.FromResult(new List<CharacterTemplateVault>() { adminCharacter });
    }
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

    [VaultColumn("bonuses")]
    public short[] Bonuses { get; set; } = Array.Empty<short>();


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
}

[Vault("npc-template")]
public class NPCVault : VaultModel
{
    // Unique template code, e.g. "orc_warrior_01"
    [VaultColumn("template-code")]
    [VaultUniqueColumn]
    public string TemplateCode { get; set; } = string.Empty;

    // Displayed name, e.g. "Orc Warrior"
    [VaultColumn("name")]
    public string Name { get; set; } = string.Empty;

    // Folder or asset path to model/prefab, e.g. "monsters/orc/warrior"
    [VaultColumn("model-path")]
    public string ModelPath { get; set; } = string.Empty;

    // Core stats – these remain static and are applied at spawn time
    [VaultColumn("level")]
    public int Level { get; set; }

    [VaultColumn("base-hp")]
    public int BaseHP { get; set; }

    [VaultColumn("base-attack")]
    public int BaseAttack { get; set; }

    [VaultColumn("base-defense")]
    public int BaseDefense { get; set; }

    // Generic property bag (mirrors your CharacterBase.Properties)
    // Represented as smallint[] in PostgreSQL
    [VaultColumn("properties")]
    public short[] Properties { get; set; } = Array.Empty<short>();

    // Tags for behavior (optional, flexible)
    // e.g. ["aggressive", "undead", "humanoid"]
    [VaultColumn("tags")]
    public string[] Tags { get; set; } = Array.Empty<string>();

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };
}

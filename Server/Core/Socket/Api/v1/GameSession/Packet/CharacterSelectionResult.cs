using System.Text.Json.Serialization;

using Altruist;

using MessagePack;

using Server.Data;

namespace Server.GameSession;

[MessagePackObject]
public struct CharacterSummary
{
    [JsonPropertyName("id")]
    [Key(0)]
    public string Id { get; set; }

    [JsonPropertyName("name")]
    [Key(1)]
    public string Name { get; set; }

    /// <summary>
    /// Packed properties indexed by CharacterProperty enum.
    /// Example: Properties[(int)CharacterProperty.Level] = 10;
    /// </summary>
    [JsonPropertyName("properties")]
    [Key(2)]
    public short[] Properties { get; set; }

    /// <summary>
    /// Current world / shard / zone the character is in.
    /// </summary>
    [JsonPropertyName("world")]
    [Key(3)]
    public string? World { get; set; }

    public CharacterSummary()
    {
        Id = string.Empty;
        Name = string.Empty;
        Properties = Array.Empty<short>();
        World = null;
    }

    public CharacterSummary(
        string id,
        string name,
        short[] properties,
        string? world = null)
    {
        Id = id;
        Name = name;
        Properties = properties ?? Array.Empty<short>();
        World = world;
    }

    public short this[CharacterProperty property]
    {
        readonly get
        {
            var index = (int)property;
            if (Properties == null || index < 0 || index >= Properties.Length)
                return 0;
            return Properties[index];
        }
        set
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
}

[MessagePackObject]
public struct SelectableCharactersResult : IPacketBase
{
    [JsonPropertyName("header")]
    [Key(0)]
    public PacketHeader Header { get; set; }

    [JsonPropertyName("type")]
    [Key(1)]
    public string Type { get; set; }

    [JsonPropertyName("characters")]
    [Key(2)]
    public CharacterSummary[] Characters { get; set; }

    public SelectableCharactersResult()
    {
        Characters = Array.Empty<CharacterSummary>();
        Type = nameof(SelectableCharactersResult);
    }

    public SelectableCharactersResult(
        CharacterSummary[] characters)
    {
        Characters = characters ?? Array.Empty<CharacterSummary>();
        Type = nameof(SelectableCharactersResult);
    }
}

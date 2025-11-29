

using System.Text.Json.Serialization;

using Altruist;

using MessagePack;

namespace Server.Packet;

[MessagePackObject]
public struct CharacterJoinedPacket : IPacketBase
{
    [Key(0)]
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [Key(1)]
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [Key(2)]
    [JsonPropertyName("properties")]
    public short[] Properties { get; set; }

    public CharacterJoinedPacket(string id, string name, short[] properties)
    {
        Id = id;
        Name = name;
        Properties = properties;
    }
}

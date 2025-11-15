using System.Text.Json.Serialization;

using Altruist;

using MessagePack;

namespace Server.GameSession;

[MessagePackObject]
public struct ServerSummary
{
    [JsonPropertyName("id")]
    [Key(0)]
    public string Id { get; set; }

    [JsonPropertyName("name")]
    [Key(1)]
    public string Name { get; set; }

    [JsonPropertyName("host")]
    [Key(2)]
    public string Host { get; set; }

    [JsonPropertyName("port")]
    [Key(3)]
    public int Port { get; set; }

    public ServerSummary(
        string id,
        string name,
        string host,
        int port)
    {
        Id = id;
        Name = name;
        Host = host;
        Port = port;
    }
}

[MessagePackObject]
public struct AvailableServerResult : IPacketBase
{
    [JsonPropertyName("header")]
    [Key(0)]
    public PacketHeader Header { get; set; }
    [JsonPropertyName("type")]
    [Key(1)]
    public string Type { get; set; }

    [JsonPropertyName("servers")]
    [Key(2)]
    public ServerSummary[] Servers { get; set; }

    public AvailableServerResult(
        ServerSummary[] servers)
    {
        Servers = servers ?? Array.Empty<ServerSummary>();
        Type = nameof(AvailableServerResult);
    }
}

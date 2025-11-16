using System.Text.Json.Serialization;

using Server.Persistence;

namespace Server;

public struct JoinServerRequest
{
    [JsonPropertyName("serverId")]
    public string ServerId { get; set; }
}

public struct AvailableServerInfo
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("name")]
    public string Name { get; set; } = "localhost";

    [JsonPropertyName("host")]
    public string Host { get; set; } = "localhost";

    [JsonPropertyName("port")]
    public int Port { get; set; } = 8000;

    [JsonPropertyName("status")]
    public string Status { get; set; } = "online";

    [JsonPropertyName("socketUrl")]
    public string SocketUrl { get; set; } = "ws://localhost:8000/ws/game";

    [JsonPropertyName("capacity")]
    public int Capacity { get; set; } = 0;

    public AvailableServerInfo(GameServer server, int actiualCapacity)
    {
        Id = server.StorageId;
        Name = server.Name;
        Host = server.Host;
        Port = server.Port;
        Status = server.Status;
        SocketUrl = server.SocketUrl;
        Capacity = actiualCapacity;
    }
}

using System.Text.Json.Serialization;

using Server.Persistence;

namespace Server;

public struct JoinServerRequest
{
    [JsonPropertyName("serverId")]
    public string ServerId { get; set; }
}

public struct JoinServerResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("host")]
    public string Host { get; set; }

    [JsonPropertyName("port")]
    public int Port { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; }

    public JoinServerResponse(AvailableServerInfo server)
    {
        Id = server.Id;
        Host = server.Host;
        Port = server.Port;
        Status = server.Status;
    }
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

    [JsonPropertyName("capacity")]
    public int Capacity { get; set; } = 0;

    public AvailableServerInfo(GameServerVault server, int actiualCapacity)
    {
        Id = server.StorageId;
        Name = server.Name;
        Host = server.Host;
        Port = server.Port;
        Status = server.Status;
        Capacity = actiualCapacity;
    }
}

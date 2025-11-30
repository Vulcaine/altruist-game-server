using System.Text.Json.Serialization;

namespace Server;

public struct JoinGameRequest
{
    [JsonPropertyName("serverId")]
    public string ServerId { get; set; }

    [JsonPropertyName("characterId")]
    public string CharacterId { get; set; }
}

public struct JoinGameResponse
{
    [JsonPropertyName("websocketUrl")]
    public string WebsocketUrl { get; set; }

    public JoinGameResponse(string websocketUrl)
    {
        WebsocketUrl = websocketUrl;
    }
}

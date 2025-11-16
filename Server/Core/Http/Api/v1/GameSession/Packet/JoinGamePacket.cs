namespace Server;

public struct JoinGameRequest
{
    public string CharacterId { get; set; }
}

public struct JoinGameResponse
{
    public string WebsocketUrl { get; set; }

    public JoinGameResponse(string websocketUrl)
    {
        WebsocketUrl = websocketUrl;
    }
}

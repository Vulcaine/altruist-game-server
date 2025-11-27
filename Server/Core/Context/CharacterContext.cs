public class PlayerSessionContext
{
    public string CharacterId { get; set; }
    public string ServerId { get; set; }

    public PlayerSessionContext(string characterId, string serverId)
    {
        CharacterId = characterId;
        ServerId = serverId;
    }
}

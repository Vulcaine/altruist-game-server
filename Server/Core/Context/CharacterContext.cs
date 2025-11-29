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

public class CharacterSessionContext
{
    public string AccountId { get; set; }
    public string CharacterId { get; set; }
    public string ServerId { get; set; }
    public int WorldIndex { get; set; }

    public CharacterSessionContext(
        string accountId,
        string characterId,
        string serverId,
        int worldIndex
        )
    {
        AccountId = accountId;
        CharacterId = characterId;
        ServerId = serverId;
        WorldIndex = worldIndex;
    }
}

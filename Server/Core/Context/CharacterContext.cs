public class GameSessionContext
{
    public string CharacterId { get; set; }
    public string AccountId { get; set; }
    public string ServerId { get; set; }

    public GameSessionContext(
        string characterId,
        string accountId, string serverId)
    {
        CharacterId = characterId;
        AccountId = accountId;
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

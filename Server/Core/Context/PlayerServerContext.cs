namespace Server;

public class PlayerServerSessionContext
{
    public string AccountId;
    public string ServerId;

    public PlayerServerSessionContext(string accountId, string serverId)
    {
        AccountId = accountId;
        ServerId = serverId;
    }
}

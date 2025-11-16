namespace Server.GameSession;

public class AccountSessionContext
{
    public string AccountId { get; set; }

    public AccountSessionContext(string accountId)
    {
        AccountId = accountId;
    }
}

using Altruist;
using Altruist.UORM;

namespace Server.Persistence;

[Vault("players")]
public class Player : VaultModel
{
    [VaultColumn("name")]
    public string Name { get; set; } = "";

    [VaultColumn("character")]
    [VaultForeignKey(typeof(Character), nameof(Character.StorageId))]
    public string CharacterId { get; set; } = "";
}

[Vault("player-server-session")]
public class PlayerServerSession : VaultModel
{
    [VaultUniqueColumn]
    [VaultColumn("account-id")]
    [VaultForeignKey(typeof(Account), nameof(Account.StorageId))]
    public string AccountId
    {
        get; set;
    } = "";

    [VaultColumn("server-id")]
    [VaultForeignKey(typeof(GameServer), nameof(GameServer.StorageId))]
    public string ServerId
    {
        get; set;
    } = "";

    [VaultColumn("session-id")]
    public string SessionId
    {
        get; set;
    } = "";

    [VaultColumn("expire-at")]
    public DateTime ExpireAt { get; set; }

    public PlayerServerSession(string accountId, string serverId, string sessionId, DateTime? expireAt = null)
    {
        AccountId = accountId;
        ServerId = serverId;
        SessionId = sessionId;
        ExpireAt = expireAt ?? DateTime.UtcNow.AddDays(1);
    }
}

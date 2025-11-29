using Altruist;
using Altruist.UORM;

namespace Server.Persistence;

[Vault("players")]
public class PlayerVault : VaultModel
{
    [VaultColumn("name")]
    public string Name { get; set; } = "";

    [VaultColumn("character")]
    [VaultForeignKey(typeof(CharacterVault), nameof(CharacterVault.StorageId))]
    public string CharacterId { get; set; } = "";
}

// [Vault("player-server-session")]
// public class PlayerServerSession : VaultModel
// {
//     [VaultUniqueColumn]
//     [VaultColumn("account-id")]
//     [VaultForeignKey(typeof(AccountVault), nameof(AccountVault.StorageId))]
//     public string AccountId
//     {
//         get; set;
//     } = "";

//     [VaultColumn("server-id")]
//     [VaultForeignKey(typeof(GameServerVault), nameof(GameServerVault.StorageId))]
//     public string ServerId
//     {
//         get; set;
//     } = "";

//     [VaultColumn("session-id")]
//     public string SessionId
//     {
//         get; set;
//     } = "";

//     [VaultColumn("expire-at")]
//     public DateTime ExpireAt { get; set; } = DateTime.UtcNow.AddDays(1);

//     public PlayerServerSession(string accountId, string serverId, string sessionId, DateTime? expireAt = null)
//     {
//         AccountId = accountId;
//         ServerId = serverId;
//         SessionId = sessionId;
//         ExpireAt = expireAt ?? DateTime.UtcNow.AddDays(1);
//     }

//     public PlayerServerSession()
//     {

//     }
// }

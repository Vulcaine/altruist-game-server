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
    [VaultColumn("player-id")]
    public string PlayerId
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
}

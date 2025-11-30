using Altruist;
using Altruist.UORM;

namespace Server.Persistence;

[Vault("players")]
public class PlayerVault : VaultModel
{
    [VaultColumn("name")]
    public string Name { get; set; } = "";

    [VaultColumn("character")]
    [VaultForeignKey(typeof(CharacterVault), nameof(StorageId))]
    public string CharacterId { get; set; } = "";
}

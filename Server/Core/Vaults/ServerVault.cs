
using Altruist;
using Altruist.Persistence;
using Altruist.UORM;

namespace Server.Persistence;

[Vault("servers")]
public class GameServer : VaultModel, IOnVaultCreate<GameServer>
{
    [VaultColumn("name")]
    public string Name { get; set; }

    [VaultColumn("host")]
    public string Host { get; set; }

    [VaultColumn("port")]
    public int Port { get; set; }

    public Task<List<GameServer>> OnCreateAsync(IServiceProvider serviceProvider)
    {
        var server = new GameServer() { Name = "localhost", Host = "localhost", Port = 8000 };
        return Task.FromResult(new List<GameServer>() { server });
    }
}

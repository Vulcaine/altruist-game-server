
using Altruist;
using Altruist.UORM;

namespace Server.Persistence;

[Vault("servers")]
public class GameServerVault : VaultModel
{
    [VaultColumn("name")]
    public string Name { get; set; } = "localhost";

    [VaultColumn("host")]
    public string Host { get; set; } = "localhost";

    [VaultColumn("port")]
    public int Port { get; set; } = 8000;

    [VaultColumn("status")]
    public string Status { get; set; } = "online";

    [VaultColumn("socket-url")]
    public string SocketUrl { get; set; } = "ws://localhost:8000/ws/game";

    [VaultColumn("capacity")]
    public int Capacity { get; set; } = 50;
}

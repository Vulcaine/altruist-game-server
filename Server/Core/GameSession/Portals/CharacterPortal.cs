
using Altruist;
using Altruist.Security;

using Server.GameSession;
using Server.Persistence;

[SessionShield]
[Portal("/game")]
public class ServerPortal : Portal
{
    private readonly IVault<GameServer> _serverVault;

    public ServerPortal(IVault<GameServer> serverVault)
    {
        _serverVault = serverVault;
    }

    [Gate("available-servers")]
    public async Task<IResultPacket> AvailableServersAsync(string clientId)
    {
        var allServers = await _serverVault.ToListAsync();
        var serverResultPacket = new AvailableServerResult(
            servers: [.. allServers
                .Select(s => new ServerSummary(
                    id: s.StorageId,
                    name: s.Name,
                    host: s.Host,
                    port: s.Port
                ))]
        );
        return ResultPacket.Success(TransportCode.Accepted, serverResultPacket);
    }
}

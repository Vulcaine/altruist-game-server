using Altruist;
using Altruist.Security;

using Microsoft.AspNetCore.Mvc;

using Server;
using Server.Persistence;

[ApiController]
[Route("/api/v1/server")]
public sealed class ServerController : BaseSessionController
{
    private readonly IVault<GameServer> _serverVault;
    private readonly IVault<PlayerServerSession> _playerServerSessonVault;

    public ServerController(IVault<GameServer> serverVault, IVault<PlayerServerSession> playerServerSessonVault)
    {
        _serverVault = serverVault;
        _playerServerSessonVault = playerServerSessonVault;
    }

    [HttpGet]
    public async Task<IActionResult> GetAvailableServers()
    {
        var allServers = await _serverVault.ToListAsync();

        var tasks = allServers.Select(async server =>
            {
                var sessionCountForServer = await _playerServerSessonVault
                    .Where(ps => ps.ServerId == server.StorageId)
                    .CountAsync();

                return new AvailableServerInfo(server, server.Capacity - (int)sessionCountForServer);
            }).ToArray();

        AvailableServerInfo[] serverInfos = await Task.WhenAll(tasks);

        return Ok(serverInfos);
    }

    [JwtShield]
    [HttpPost("join")]
    public async Task<IActionResult> JoinServer([FromBody] JoinServerRequest request)
    {
        var accountId = GetAccountId();
        if (string.IsNullOrEmpty(accountId))
            return Unauthorized("Missing principal id.");

        var existingServer = await _serverVault
            .Where(s => s.StorageId == request.ServerId)
            .FirstOrDefaultAsync();

        if (existingServer == null)
        {
            return BadRequest("Server not found");
        }

        if (existingServer.Status != GameServerStatus.Online)
        {
            return BadRequest("Server is not online.");
        }

        var sessionCountForServer = await _playerServerSessonVault
            .Where(s => s.ServerId == existingServer.StorageId)
            .CountAsync();

        if (existingServer.Capacity <= sessionCountForServer)
        {
            return BadRequest("Server is full.");
        }

        var existingSession = await _playerServerSessonVault
            .Where(s => s.AccountId == accountId && s.ServerId == existingServer.StorageId)
            .FirstOrDefaultAsync();

        var expired = existingSession?.ExpireAt < DateTime.UtcNow;

        if (existingSession != null && !expired)
            return Ok(existingSession);

        var serverSession = new PlayerServerSession(accountId, existingServer.StorageId, Guid.NewGuid().ToString());
        await _playerServerSessonVault.SaveAsync(serverSession);

        return Ok(serverSession);
    }
}

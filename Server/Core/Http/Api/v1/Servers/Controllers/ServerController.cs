using Altruist;
using Altruist.Security;

using Microsoft.AspNetCore.Mvc;

using Server;
using Server.Persistence;

[ApiController]
[Route("/api/v1/server")]
public sealed class ServerController : BaseSessionController
{
    private readonly IVault<GameServerVault> _serverVault;
    private readonly IGameSessionService _gameSessionService;

    public ServerController(IVault<GameServerVault> serverVault, IGameSessionService gameSessionService)
    {
        _serverVault = serverVault;
        _gameSessionService = gameSessionService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAvailableServers()
    {
        var allServers = await _serverVault.ToListAsync();

        AvailableServerInfo[] serverInfos = allServers.Select(server =>
            {
                // TODO find all sessions instead of subtracting 0
                return new AvailableServerInfo(server, server.Capacity - (int)0);
            }).ToArray();

        return Ok(serverInfos);
    }

    [JwtShield]
    [HttpPost("join")]
    public async Task<IActionResult> JoinServer([FromBody] JoinServerRequest request)
    {
        var accountId = GetAccountId();
        if (string.IsNullOrEmpty(accountId))
            return Unauthorized("Missing principal id.");

        var session = _gameSessionService.GetSession(accountId);

        if (session == null)
        {
            return BadRequest("You are not joined to any game.");
        }

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

        var sessionCountForServer = 0; // TODO: find real session count

        if (existingServer.Capacity <= sessionCountForServer)
        {
            return BadRequest("Server is full.");
        }

        var existingSession = session.GetContext<PlayerServerSessionContext>(accountId);

        if (existingSession != null)
        {
            var existingResponse = new JoinServerResponse(
                new AvailableServerInfo(existingServer, existingServer.Capacity - sessionCountForServer)
            );
            return Ok(existingResponse);
        }

        var serverSession = new PlayerServerSessionContext(accountId, existingServer.StorageId);

        await session.SetContext(accountId, serverSession);

        var response = new JoinServerResponse(
            new AvailableServerInfo(existingServer, existingServer.Capacity - sessionCountForServer)
        );

        return Ok(response);
    }
}

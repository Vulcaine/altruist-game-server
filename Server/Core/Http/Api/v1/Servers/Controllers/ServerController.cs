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
        var allServerSessions = _gameSessionService.FindAllContexts<PlayerServerSessionContext>();

        AvailableServerInfo[] serverInfos = allServers.Select(server =>
            {
                var contextsForThisServer = allServerSessions.Where(s => s.ServerId == server.StorageId).Count();
                return new AvailableServerInfo(server, server.Capacity - contextsForThisServer);
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
            return Unauthorized("You are not logged in.");
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

        var sessionCountForServer = existingServer.Capacity - session.FindAllContexts<PlayerServerSessionContext>().Count();

        if (sessionCountForServer == 0)
        {
            return BadRequest("Server is full.");
        }

        var existingSession = await session.GetContext<PlayerServerSessionContext>(accountId);

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

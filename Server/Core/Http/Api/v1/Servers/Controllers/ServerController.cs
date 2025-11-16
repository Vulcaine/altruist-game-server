using Altruist;

using Microsoft.AspNetCore.Mvc;

using Server.Persistence;

[ApiController]
[Route("/api/v1/server")]
public sealed class ServerController : ControllerBase
{
    private readonly IVault<GameServer> _serverVault;
    public ServerController(IVault<GameServer> serverVault)
    {
        _serverVault = serverVault;
    }

    [HttpGet]
    public async Task<IActionResult> GetAvailableServers()
    {
        var allServers = await _serverVault.ToListAsync();
        return Ok(allServers);
    }
}

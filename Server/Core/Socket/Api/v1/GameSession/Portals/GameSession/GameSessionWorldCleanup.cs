using Altruist;
using Altruist.Gaming.ThreeD;

namespace Server.GameSession;

public interface IGameSessionWorldCleanup
{
    Task CleanupOnDisconnectAsync(string clientId, Exception? exception);
}

[Service(typeof(IGameSessionWorldCleanup))]
public class GameSessionWorldCleanup : IGameSessionWorldCleanup
{
    private readonly IGameSessionService _gameSessionService;
    private readonly IGameWorldOrganizer3D _gameWorldOrganizer;

    public GameSessionWorldCleanup(
        IGameSessionService gameSessionService,
        IGameWorldOrganizer3D gameWorldOrganizer)
    {
        _gameSessionService = gameSessionService;
        _gameWorldOrganizer = gameWorldOrganizer;
    }

    public async Task CleanupOnDisconnectAsync(string clientId, Exception? exception)
    {
        var clientSession = _gameSessionService.GetSession(clientId);
        if (clientSession == null)
            return;

        var characterSession =
         clientSession.GetContext<CharacterSessionContext>(clientId);

        if (characterSession == null)
            return;

        var characterWorld = _gameWorldOrganizer.GetWorld(characterSession.WorldIndex);
        if (characterWorld == null)
            return;

        characterWorld.DestroyObject(characterSession.CharacterId);
    }
}

using Altruist;
using Altruist.Gaming;

namespace Server.GameSession;

[Portal("/game")]
public class GameSession : AltruistGameSessionPortal
{
    public GameSession(IGameSessionService gameSessionService) : base(gameSessionService)
    {
    }
}

using Altruist;
using Altruist.Gaming;
using Altruist.Security;

using Server.Persistence;

namespace Server.GameSession;

[SessionShield]
[Portal("/game")]
public class GameSession : AltruistGameSessionPortal
{
    private readonly IVault<Character> _characterVault;
    public GameSession(IGameSessionService gameSessionService, IAltruistRouter router, IVault<Character> characterVault) : base(gameSessionService, router)
    {
        _characterVault = characterVault;
    }

    protected override async Task<IResultPacket> OnHandshakeReceived(HandshakePacket message, string clientId, IResultPacket result)
    {
        return result;
    }

    [Gate("character-selection")]
    public async Task<IResultPacket> CharacterSelectionAsync(string clientId)
    {
        AccountSessionContext? clientAccountContext =
            await _gameSessionService.GetContext<AccountSessionContext>(clientId);

        if (clientAccountContext == null)
        {
            return ResultPacket.Failed(
                TransportCode.Unauthorized,
                reason: "You are not logged in!");
        }

        var allCharacters = await _characterVault
            .Where(c => c.AccountId == clientAccountContext.AccountId)
            .ToListAsync();

        var summaries = allCharacters
            .Select(c => new CharacterSummary(
                id: c.StorageId,
                name: c.Name,
                properties: c.Properties ?? Array.Empty<short>(),
                world: c.World
            ))
            .ToArray();

        var packet = new SelectableCharactersResult(
            characters: summaries
        );

        return ResultPacket.Success(TransportCode.Accepted, packet);
    }

    [Gate("join-world")]
    public Task JoinWorldAsync()
    {
        return Task.CompletedTask;
    }
}

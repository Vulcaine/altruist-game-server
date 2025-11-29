using System.Numerics;

using Altruist;
using Altruist.Gaming.ThreeD;
using Altruist.Numerics;
using Altruist.Persistence;
using Altruist.Security;

using Server.Packet;

namespace Server.GameSession;

[SessionShield]
[Portal("/game/v1")]
public class GameSessionPortal : BaseSessionPortal
{
    private readonly IGameCharacterSessionService _characterSessionService;
    private readonly IGameWorldOrganizer3D _gameWorldOrganizer;
    private readonly ISpatialBroadcastService3D _spatialBroadcastService;
    private readonly IGameMovementSessionService _movementSessionService;

    private readonly IPrefabVault<CharacterPrefab> _characterPrefabVault;

    public GameSessionPortal(
        IGameSessionService gameSessionService,
        IGameWorldOrganizer3D gameWorldOrganizer,
        IAltruistRouter router,
        ISessionTokenValidator tokenValidator,
        IGameCharacterSessionService characterSessionService,
        ISpatialBroadcastService3D spatialBroadcastService,
        IGameMovementSessionService movementSessionService,
        IPrefabVault<CharacterPrefab> characterPrefabVault
    ) : base(gameSessionService, router, tokenValidator)
    {
        _characterSessionService = characterSessionService;
        _gameWorldOrganizer = gameWorldOrganizer;
        _spatialBroadcastService = spatialBroadcastService;
        _movementSessionService = movementSessionService;
        _characterPrefabVault = characterPrefabVault;
    }

    [Cycle("*/30 * * * * *")]
    public async Task PersistCharacterPositionsSnapshot()
    {
        foreach (var world in _gameWorldOrganizer.GetAllWorlds())
        {
            var allCharacterPrefabs = world.FindAllObjects<CharacterPrefab>();

            foreach (var characterPrefab in allCharacterPrefabs)
            {
                var characterVault = await characterPrefab.Character.LoadAsync();
                if (characterVault == null)
                    continue;

                var playerId = characterVault.StorageId;

                if (!_movementSessionService.TryGetPlayerState(playerId, out var state))
                    continue;

                characterVault.X = (int)state.Position.X;
                characterVault.Y = (int)state.Position.Y;
                characterVault.Z = (int)state.Position.Z;

                var (yaw, pitch, roll) = ToYawPitchRoll(state.Orientation);
                characterVault.Yaw = yaw;
                characterVault.Pitch = pitch;
                characterVault.Roll = roll;

                await _characterPrefabVault.SaveAsync(characterPrefab);
            }
        }
    }

    [Cycle]
    public async Task UpdatePlayerPosition()
    {
        foreach (var world in _gameWorldOrganizer.GetAllWorlds())
        {
            var allCharacterPrefabs = world.FindAllObjects<CharacterPrefab>();

            foreach (var characterPrefab in allCharacterPrefabs)
            {
                var characterVault = await characterPrefab.Character.LoadAsync();
                if (characterVault == null)
                    continue;

                var playerId = characterVault.StorageId;

                if (!_movementSessionService.TryGetPlayerState(playerId, out var state))
                    continue;

                var pos = state.Position;
                var (yaw, pitch, roll) = ToYawPitchRoll(state.Orientation);

                var update = new UpdateClientPositionAndOrientation(
                    pos.X,
                    pos.Y,
                    pos.Z,
                    yaw,
                    pitch,
                    roll,
                    characterPrefab.StateFlags
                );

                var cell = new IntVector3(
                    (int)pos.X,
                    (int)pos.Y,
                    (int)pos.Z
                );

                _ = _spatialBroadcastService.SpatialBroadcast<CharacterPrefab>(
                    world.Index.Index,
                    cell,
                    update);
            }
        }
    }

    private static (float Yaw, float Pitch, float Roll) ToYawPitchRoll(Quaternion q)
    {
        q = Quaternion.Normalize(q);

        float siny_cosp = 2f * (q.W * q.Y + q.Z * q.X);
        float cosy_cosp = 1f - 2f * (q.Y * q.Y + q.Z * q.Z);
        float yaw = MathF.Atan2(siny_cosp, cosy_cosp);

        float sinp = 2f * (q.W * q.X - q.Z * q.Y);
        float pitch;
        if (MathF.Abs(sinp) >= 1f)
            pitch = MathF.CopySign(MathF.PI / 2f, sinp);
        else
            pitch = MathF.Asin(sinp);

        float sinr_cosp = 2f * (q.W * q.Z + q.X * q.Y);
        float cosr_cosp = 1f - 2f * (q.Z * q.Z + q.X * q.X);
        float roll = MathF.Atan2(sinr_cosp, cosr_cosp);

        return (yaw, pitch, roll);
    }

    protected override async Task<IResultPacket> OnHandshakeReceived(
        HandshakeRequestPacket message,
        string clientId,
        IResultPacket result)
    {
        var accountId = await GetAccountId(message.Token);

        var authError = EnsureAuthenticated(accountId);
        if (authError != null)
            return authError;

        var clientSession = EnsureClientSession(accountId!, clientId);
        if (clientSession == null)
            return Unauthorized("You are not joined to any game.");

        return await _characterSessionService.HandleHandshakeForSessionAsync(
            clientId,
            accountId!,
            clientSession,
            result);
    }

    private static IResultPacket Unauthorized(string message)
        => ResultPacket.Failed(TransportCode.Unauthorized, message);

    private IResultPacket? EnsureAuthenticated(string? accountId)
    {
        if (accountId == null)
            return Unauthorized("Invalid token");

        return null;
    }

    private IGameSession? EnsureClientSession(string accountId, string clientId)
    {
        var accountSession = _gameSessionService.GetSession(accountId);
        if (accountSession == null)
            return null;

        var expiresAt = DateTime.UtcNow.AddHours(GameSessionConstants.SessionExpirationHours);
        return _gameSessionService.MigrateSession(accountId, clientId, expiresAt);
    }
}

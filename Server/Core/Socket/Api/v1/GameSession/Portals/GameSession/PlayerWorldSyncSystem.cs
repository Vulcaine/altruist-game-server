using Altruist;
using Altruist.Gaming.ThreeD;
using Altruist.Numerics;
using Altruist.Persistence;

using Server.Packet;

namespace Server.GameSession;

/// <summary>
/// Periodic world sync: saving character snapshots and broadcasting player positions.
/// Driven by the engine via [Cycle] attributes.
/// </summary>
[Service]
public class PlayerWorldSyncSystem
{
    private readonly IGameWorldOrganizer3D _gameWorldOrganizer;
    private readonly ISpatialBroadcastService3D _spatialBroadcastService;
    private readonly IPrefabVault<CharacterPrefab> _characterPrefabVault;

    public PlayerWorldSyncSystem(
        IGameWorldOrganizer3D gameWorldOrganizer,
        ISpatialBroadcastService3D spatialBroadcastService,
        IPrefabVault<CharacterPrefab> characterPrefabVault)
    {
        _gameWorldOrganizer = gameWorldOrganizer;
        _spatialBroadcastService = spatialBroadcastService;
        _characterPrefabVault = characterPrefabVault;
    }

    [Cycle(CronPresets.EveryMinute)]
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

                await _characterPrefabVault.SaveAsync(characterPrefab);
            }
        }
    }

    /// <summary>
    /// Every engine tick, broadcast player positions and orientations to nearby clients.
    /// </summary>
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

                var position = characterPrefab.GetCurrentPosition();
                var (yaw, pitch, roll) = characterPrefab.GetCurrentYawPitchRoll();

                var update = new UpdateClientPositionAndOrientation(
                    position.X,
                    position.Y,
                    position.Z,
                    yaw,
                    pitch,
                    roll,
                    characterPrefab.StateFlags
                );

                var cell = new IntVector3(
                    (int)position.X,
                    (int)position.Y,
                    (int)position.Z
                );

                _ = _spatialBroadcastService.SpatialBroadcast<CharacterPrefab>(
                    world.Index.Index,
                    cell,
                    update);
            }
        }
    }
}

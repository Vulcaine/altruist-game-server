
using Altruist;
using Altruist.Gaming.ThreeD;
using Altruist.Numerics;
using Altruist.Persistence;

using Server.Packet;

namespace Server.GameSystems
{
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
        private readonly IGameSessionService _sessionService;

        public PlayerWorldSyncSystem(
            IGameWorldOrganizer3D gameWorldOrganizer,
            ISpatialBroadcastService3D spatialBroadcastService,
            IPrefabVault<CharacterPrefab> characterPrefabVault,
            IGameSessionService sessionService)
        {
            _gameWorldOrganizer = gameWorldOrganizer;
            _spatialBroadcastService = spatialBroadcastService;
            _characterPrefabVault = characterPrefabVault;
            _sessionService = sessionService;
        }

        /// <summary>
        /// Persist character state snapshot periodically (e.g. once per minute).
        /// </summary>
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
        /// Uses per-session snapshot caching to avoid sending redundant updates.
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

                    var clientId = characterPrefab.ClientId;
                    if (string.IsNullOrWhiteSpace(clientId))
                        continue;

                    var session = _sessionService.GetSession(clientId);
                    if (session == null)
                        continue;

                    var position = characterPrefab.GetCurrentPosition();
                    var (yaw, pitch, roll) = characterPrefab.GetCurrentYawPitchRoll();

                    var newSnapshot = new UpdateClientPositionAndOrientation(
                        position.X,
                        position.Y,
                        position.Z,
                        yaw,
                        pitch,
                        roll,
                        characterPrefab.StateFlags
                    );

                    UpdateClientPositionAndOrientation? lastSnapshot = session.GetContext<UpdateClientPositionAndOrientation?>(clientId);

                    if (lastSnapshot.HasValue && lastSnapshot.Value.Equals(newSnapshot))
                    {
                        continue;
                    }

                    session.SetContext(clientId, newSnapshot);

                    var cell = new IntVector3(
                        (int)position.X,
                        (int)position.Y,
                        (int)position.Z
                    );

                    _ = _spatialBroadcastService.SpatialBroadcast<CharacterPrefab>(
                        world.Index.Index,
                        cell,
                        newSnapshot);
                }
            }
        }
    }
}

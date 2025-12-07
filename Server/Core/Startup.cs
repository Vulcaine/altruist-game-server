using System.Numerics;

using Altruist;
using Altruist.Gaming.ThreeD;
using Altruist.Numerics;
using Altruist.ThreeD.Numerics;

using BepuPhysics;
using BepuPhysics.Collidables;

using BepuUtilities.Memory;

[Service]
public sealed class WorldTerrainInitializer
{
    private readonly IHeightmapLoader3D _heightmapLoader;
    private readonly BufferPool _bufferPool;

    public WorldTerrainInitializer(
        IHeightmapLoader3D heightmapLoader,
        BufferPool bufferPool)
    {
        _heightmapLoader = heightmapLoader;
        _bufferPool = bufferPool;
    }

    public Mesh LoadTerrain(HeightmapData heightmapData)
    {
        var mesh = _heightmapLoader.LoadHeightmapMesh(heightmapData, _bufferPool);
        return mesh;
    }
}


[AltruistModule]
public static class ServerModule
{
    private sealed class Plane : WorldObject3D
    {
        public Plane(Transform3D transform, string zoneId = "", string? archetype = null)
            : base(transform, zoneId, archetype)
        {
        }
    }

    [AltruistModuleLoader]
    public static void Initialize(IGameWorldOrganizer3D worldOrganizer, WorldTerrainInitializer worldTerrainInitializer)
    {
        var allWorlds = worldOrganizer.GetAllWorlds();

        foreach (var world in allWorlds)
        {
            var worldSize = world.Index.Size;
            var origin = new IntVector3(0, 0, 0);
            var size = new Vector3(
                x: worldSize.X,
                y: 5f,
                z: worldSize.Z
            );

            var transform = Transform3D.From(
                origin: origin,
                rotation: Quaternion.Identity,
                size: size
            );

            var mesh = worldTerrainInitializer.LoadTerrain(
                world.HeightmapData
            );
            world.SpawnStaticObject(mesh);
        }
    }
}

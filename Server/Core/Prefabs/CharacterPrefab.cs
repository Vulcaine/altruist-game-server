using Altruist.Gaming;
using Altruist.Gaming.ThreeD;

using Server.Persistence;
using Altruist.Persistence;
using Altruist;

[Prefab("character")]
[WorldObject("character")]
public class CharacterPrefab : WorldObjectPrefab3D
{
    [PrefabComponent]
    public IPrefabHandle<CharacterVault> Character { get; set; } = default!;

    [PostConstruct]
    public void Init()
    {
        const float radius = 0.5f;
        const float halfLength = 1.0f;

        var bodyProfile = new HumanoidCapsuleBodyProfile(radius, halfLength, 75f);
        BodyDescriptor = bodyProfile.CreateBody(Transform);
    }
}

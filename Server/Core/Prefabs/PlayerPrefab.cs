using Altruist;
using Altruist.Gaming;
using Altruist.Gaming.ThreeD;

using Server.Data;
using Server.Persistence;

[Prefab("character")]
[WorldObject("character")]
public class CharacterPrefab : PrefabWorldObject3D
{
    private CharacterVault? _character;

    [PostConstruct]
    public void Init(IPrefabEditor3D editor)
    {
        _character = editor.Edit(this).Add<CharacterVault>();

        const float radius = 0.5f;
        const float halfLength = 1.0f;

        // Use a body profile for a humanoid capsule
        var bodyProfile = new HumanoidCapsuleBodyProfile(radius, halfLength, 75f);
        BodyDescriptor = bodyProfile.CreateBody(Transform);
    }

    private CharacterVault Character => _character
        ?? throw new InvalidOperationException(
            "CharacterPrefab not initialized. Ensure Init(IPrefabEditor3D) ran before accessing character data.");

    // -----------------------
    // Delegated properties
    // -----------------------

    public string AccountId
    {
        get => Character.AccountId;
        set => Character.AccountId = value;
    }

    public string Name
    {
        get => Character.Name;
        set => Character.Name = value;
    }

    /// <summary>
    /// Current world / shard / zone the character is in.
    /// </summary>
    public string World
    {
        get => Character.World;
        set => Character.World = value;
    }

    /// <summary>
    /// Template code this character is based on.
    /// Usually fixed after creation, but exposed in case migration/tools need it.
    /// </summary>
    public string TemplateCode
    {
        get => Character.TemplateCode;
        set => Character.TemplateCode = value;
    }

    /// <summary>
    /// Game server this character belongs to.
    /// </summary>
    public string ServerId
    {
        get => Character.ServerId;
        set => Character.ServerId = value;
    }

    // -----------------------
    // Packed properties API
    // -----------------------

    public short GetProperty(CharacterProperty property)
        => Character.GetProperty(property);

    public void SetProperty(CharacterProperty property, short value)
        => Character.SetProperty(property, value);

    public short Level
    {
        get => GetProperty(CharacterProperty.Level);
        set => SetProperty(CharacterProperty.Level, value);
    }
}

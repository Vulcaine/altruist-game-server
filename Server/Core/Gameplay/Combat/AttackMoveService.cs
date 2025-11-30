using Altruist;

using Server.Persistence;

namespace Server.Gameplay;

[Service]
public class AttackMoveService
{
    private readonly IVault<AttackMoveVault> _attackMoveVault;
    private IEnumerable<AttackMoveVault> _allAttackMoves = Enumerable.Empty<AttackMoveVault>();

    public AttackMoveService(IVault<AttackMoveVault> attackMoveVault)
    {
        _attackMoveVault = attackMoveVault;
    }

    [PostConstruct]
    public async Task Init()
    {
        _allAttackMoves = await _attackMoveVault.ToListAsync();
    }

    // ---------------------------
    // FACTORY METHODS (DTO side)
    // ---------------------------

    public BasicAttack CreateBasicAttack(
        string name,
        float baseDamage,
        float range,
        float cooldownSeconds,
        float staminaCost = 0f,
        float manaCost = 0f)
    {
        return new BasicAttack(
            name: name,
            baseDamage: baseDamage,
            range: range,
            cooldownSeconds: cooldownSeconds,
            manaCost: manaCost,
            staminaCost: staminaCost);
    }

    public Spell CreateSpell(
        string name,
        float baseDamage,
        float range,
        float cooldownSeconds,
        float castTimeSeconds = 0f,
        float manaCost = 0f,
        float staminaCost = 0f,
        bool requiresTarget = true)
    {
        return new Spell(
            name: name,
            baseDamage: baseDamage,
            range: range,
            cooldownSeconds: cooldownSeconds,
            castTimeSeconds: castTimeSeconds,
            manaCost: manaCost,
            staminaCost: staminaCost,
            requiresTarget: requiresTarget);
    }

    public ProjectileSpell CreateProjectileSpell(
        string name,
        float baseDamage,
        float range,
        float cooldownSeconds,
        float projectileSpeed,
        float castTimeSeconds = 0f,
        float explosionRadius = 0f,
        float manaCost = 0f,
        float staminaCost = 0f,
        bool requiresTarget = false)
    {
        return new ProjectileSpell(
            name: name,
            baseDamage: baseDamage,
            range: range,
            cooldownSeconds: cooldownSeconds,
            projectileSpeed: projectileSpeed,
            castTimeSeconds: castTimeSeconds,
            explosionRadius: explosionRadius,
            manaCost: manaCost,
            staminaCost: staminaCost,
            requiresTarget: requiresTarget);
    }

    public AoESpell CreateAoESpell(
        string name,
        float baseDamage,
        float range,
        float cooldownSeconds,
        float areaRadius,
        float coneAngleDegrees = 0f,
        float durationSeconds = 0f,
        float tickIntervalSeconds = 0f,
        float channelDurationSeconds = 0f,
        float castTimeSeconds = 0f,
        float manaCost = 0f,
        float staminaCost = 0f,
        bool requiresTarget = false)
    {
        return new AoESpell(
            name: name,
            baseDamage: baseDamage,
            range: range,
            cooldownSeconds: cooldownSeconds,
            areaRadius: areaRadius,
            coneAngleDegrees: coneAngleDegrees,
            durationSeconds: durationSeconds,
            tickIntervalSeconds: tickIntervalSeconds,
            channelDurationSeconds: channelDurationSeconds,
            castTimeSeconds: castTimeSeconds,
            manaCost: manaCost,
            staminaCost: staminaCost,
            requiresTarget: requiresTarget);
    }

    public ComboAttack CreateComboAttack(
        string name,
        float baseDamage,
        float range,
        float cooldownSeconds,
        int steps,
        float staminaCost = 0f,
        float manaCost = 0f)
    {
        return new ComboAttack(
            name: name,
            baseDamage: baseDamage,
            range: range,
            cooldownSeconds: cooldownSeconds,
            steps: steps,
            manaCost: manaCost,
            staminaCost: staminaCost);
    }

    // ---------------------------------------
    // Mapping from Vault -> Domain DTO
    // ---------------------------------------

    // ---------------------------------------
    // Mapping from Vault -> Domain DTO
    // ---------------------------------------
    public AttackMove ToDomain(AttackMoveVault vault)
    {
        if (vault.ProjectileSpeed > 0f)
        {
            return CreateProjectileSpell(
                name: vault.Name,
                baseDamage: vault.BaseDamage,
                range: vault.Range,
                cooldownSeconds: vault.CooldownSeconds,
                projectileSpeed: vault.ProjectileSpeed,
                castTimeSeconds: vault.CastTimeSeconds,
                explosionRadius: vault.AreaRadius,
                manaCost: vault.ManaCost,
                staminaCost: vault.StaminaCost,
                requiresTarget: vault.RequiresTarget);
        }

        if (vault.AreaRadius > 0f)
        {
            return CreateAoESpell(
                name: vault.Name,
                baseDamage: vault.BaseDamage,
                range: vault.Range,
                cooldownSeconds: vault.CooldownSeconds,
                areaRadius: vault.AreaRadius,
                coneAngleDegrees: vault.ConeAngleDegrees,
                durationSeconds: vault.DurationSeconds,
                tickIntervalSeconds: vault.TickIntervalSeconds,
                channelDurationSeconds: vault.ChannelDurationSeconds,
                castTimeSeconds: vault.CastTimeSeconds,
                manaCost: vault.ManaCost,
                staminaCost: vault.StaminaCost,
                requiresTarget: vault.RequiresTarget);
        }

        if (vault.Range > 0f && (vault.CastTimeSeconds > 0f || vault.ManaCost > 0f))
        {
            return CreateSpell(
                name: vault.Name,
                baseDamage: vault.BaseDamage,
                range: vault.Range,
                cooldownSeconds: vault.CooldownSeconds,
                castTimeSeconds: vault.CastTimeSeconds,
                manaCost: vault.ManaCost,
                staminaCost: vault.StaminaCost,
                requiresTarget: vault.RequiresTarget);
        }

        return CreateBasicAttack(
            name: vault.Name,
            baseDamage: vault.BaseDamage,
            range: vault.Range,
            cooldownSeconds: vault.CooldownSeconds,
            staminaCost: vault.StaminaCost,
            manaCost: vault.ManaCost);
    }

    public IReadOnlyList<AttackMove> GetAllDomainMoves()
        => _allAttackMoves.Select(ToDomain).ToList();
}

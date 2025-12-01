using System.Text.Json;

using Altruist;
using Altruist.Persistence;
using Altruist.Security;

using Microsoft.Extensions.DependencyInjection;

using Server.Persistence;
public class ServerInitializer : IDatabaseInitializer
{
    public int Order => 0;

    public Task<IEnumerable<IVaultModel>> InitializeAsync(IServiceProvider services)
    {
        var server = new GameServerVault
        {
            Name = "localhost",
            Host = "localhost",
            Port = 8000,
            Status = "online",
            SocketUrl = "ws://localhost:8000/ws/game",
            Capacity = 50,
        };

        return Task.FromResult<IEnumerable<IVaultModel>>([server]);
    }
}
public class AccountInitializer : IDatabaseInitializer
{
    public int Order => 0;

    public Task<IEnumerable<IVaultModel>> InitializeAsync(IServiceProvider services)
    {
        var passwordHasher = services.GetRequiredService<IPasswordHasher>();

        var admin = new AccountVault
        {
            Username = "admin",
            PasswordHash = passwordHasher.Hash("admin"),
            Email = "admin@admin.com",
            EmailVerified = true
        };

        return Task.FromResult<IEnumerable<IVaultModel>>([admin]);
    }
}
public class CharacterTemplateInitializer : IDatabaseInitializer
{
    public int Order => 0;

    public Task<IEnumerable<IVaultModel>> InitializeAsync(IServiceProvider services)
    {
        var admin = new CharacterTemplateVault
        {
            TemplateCode = "character",
            Name = "Admin",
            WorldIndex = 0
        };

        return Task.FromResult<IEnumerable<IVaultModel>>([admin]);
    }
}

public class CharacterInitializer : IDatabaseInitializer
{
    public int Order => 1;

    public async Task<IEnumerable<IVaultModel>> InitializeAsync(IServiceProvider services)
    {
        var accountVault = services.GetRequiredService<IVault<AccountVault>>();
        var serverVault = services.GetRequiredService<IVault<GameServerVault>>();

        var adminAccount = await accountVault.Where(a => a.Username == "admin").FirstOrDefaultAsync();
        var localhostServer = await serverVault.Where(s => s.Name == "localhost").FirstOrDefaultAsync();

        if (adminAccount == null || localhostServer == null)
            return Array.Empty<IVaultModel>();

        var adminCharacter = new CharacterVault
        {
            AccountId = adminAccount.StorageId,
            ServerId = localhostServer.StorageId,
            Name = "Admin",
            TemplateCode = "character",
            WorldIndex = 0,
        };

        return [adminCharacter];
    }
}

public class NPCInitializer : IDatabaseInitializer
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    public int Order => 1;

    public async Task<IEnumerable<IVaultModel>> InitializeAsync(IServiceProvider services)
    {
        var filePath = Path.Combine(AppContext.BaseDirectory, "Resources", "monsters.json");

        if (!File.Exists(filePath))
            return Array.Empty<IVaultModel>();

        var json = await File.ReadAllTextAsync(filePath);
        var list = JsonSerializer.Deserialize<List<NPCVault>>(json, _jsonOptions);

        return list?.Cast<IVaultModel>() ?? Array.Empty<IVaultModel>();
    }
}


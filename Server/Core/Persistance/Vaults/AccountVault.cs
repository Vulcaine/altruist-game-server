
using Altruist.Persistence;
using Altruist.Security;
using Altruist.UORM;

using Microsoft.Extensions.DependencyInjection;

namespace Server.Persistence;

[Vault("account")]
public class AccountVault : AccountModel, IOnVaultCreate<AccountVault>
{
    [VaultUniqueColumn]
    [VaultColumn("username")]
    public string Username { get; set; } = "";

    [VaultColumn("password-hash")]
    public string PasswordHash { get; set; } = "";

    [VaultUniqueColumn]
    [VaultColumn("email")]
    public string Email { get; set; } = "";

    [VaultColumn("email-verified")]
    public bool EmailVerified { get; set; }

    [VaultColumn("email-verification-token")]
    public string EmailVerificationToken { get; set; } = "";

    public async Task<List<AccountVault>> OnCreateAsync(IServiceProvider serviceProvider)
    {
        var passwordHasher = serviceProvider.GetRequiredService<IPasswordHasher>();
        var account = new AccountVault() { Username = "admin", PasswordHash = passwordHasher.Hash("admin"), Email = "admin@admin.com" };
        return await Task.FromResult(new List<AccountVault> { account });
    }
}

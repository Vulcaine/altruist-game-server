
using Altruist.Persistence;
using Altruist.Security;
using Altruist.UORM;

namespace Server.Persistence;

[Vault("account")]
public class Account : AccountModel, IOnVaultCreate<Account>
{
    [VaultColumnIndex]
    [VaultColumn("username")]
    public string Username { get; set; }

    [VaultColumn("passwordHash")]
    public string PasswordHash { get; set; }

    [VaultColumnIndex]
    [VaultColumn("email")]
    public string Email { get; set; }

    [VaultColumn("emailVerified")]
    public bool EmailVerified { get; set; }

    [VaultColumn("emailVerificationToken")]
    public string EmailVerificationToken { get; set; }

    public async Task<List<Account>> OnCreateAsync(IServiceProvider serviceProvider)
    {
        var account = new Account() { Username = "admin", PasswordHash = "admin", Email = "admin@admin.com" };
        return await Task.FromResult(new List<Account> { account });
    }
}

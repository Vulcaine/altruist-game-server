using Altruist;
using Altruist.Persistence;
using Altruist.UORM;

namespace Server.Persistence;

[Vault("account")]
public class Account : VaultModel, IOnVaultCreate<Account>
{
    [VaultColumnIndex]
    [VaultColumn("username")]
    public string Username { get; set; }

    [VaultColumn("passwordHash")]
    public string PasswordHash { get; set; }

    [VaultColumn("email")]
    [VaultColumnIndex]
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

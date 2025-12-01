using Altruist.Security;
using Altruist.UORM;

namespace Server.Persistence;

[Vault("account")]
public class AccountVault : AccountModel
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
    public bool EmailVerified { get; set; } = false;

    [VaultColumn("email-verification-token", nullable: true)]
    public string EmailVerificationToken { get; set; } = "";

}

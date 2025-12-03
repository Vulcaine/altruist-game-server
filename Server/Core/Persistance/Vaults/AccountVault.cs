using Altruist.Security;
using Altruist.UORM;

namespace Server.Persistence;

[Vault("account", Keyspace: "account")]
[VaultUniqueKey("username")]
[VaultUniqueKey("email")]
public class AccountVault : AccountModel
{
    [VaultColumn("username")]
    public string Username { get; set; } = "";

    [VaultColumn("password-hash")]
    public string PasswordHash { get; set; } = "";

    [VaultColumn("email")]
    public string Email { get; set; } = "";

    [VaultColumn("email-verified")]
    public bool EmailVerified { get; set; } = false;

    [VaultColumn("email-verification-token", nullable: true)]
    public string EmailVerificationToken { get; set; } = "";

}

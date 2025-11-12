using Altruist;

using Microsoft.Extensions.Options;

using Server.Email;
using Server.Persistence;

namespace Server.Signup;

[Service]
public class SignupService
{
    private readonly IVault<Account> _accountVault;
    private readonly IEmailService _emailService;
    private readonly AppUrls _urls;

    public SignupService(IVault<Account> accountVault, IEmailService emailService, IOptions<AppUrls> urls)
    {
        _accountVault = accountVault;
        _emailService = emailService;
        _urls = urls.Value;
    }

    public async Task<SignupResult> Signup(string email, string username, string password)
    {
        var existing = await _accountVault
            .Where(acc => acc.Username == username || acc.Email == email)
            .FirstOrDefaultAsync();

        if (existing != null)
        {
            var reason = existing.Email == email ? "Email is already in use" : "Username is already taken";
            return SignupResult.RFailure(reason);
        }

        var token = Guid.NewGuid().ToString("N");
        var expiresAt = DateTimeOffset.UtcNow.AddHours(24);

        var account = new Account
        {
            Username = username,
            PasswordHash = password,
            Email = email,
            EmailVerified = false,
            EmailVerificationToken = token
        };

        await _accountVault.SaveAsync(account);

        var verifyLink = BuildVerificationLink(account.StorageId, token);
        await _emailService.SendVerificationEmail(email, username, verifyLink);

        var verification = new SignupResponse.VerificationInfo
        {
            Method = "email",
            SentTo = email,
            ExpiresAt = expiresAt
        };

        return SignupResult.ROk(
            userId: account.StorageId,
            username: account.Username,
            email: account.Email,
            requiresEmailVerification: true,
            verification: verification
        );
    }

    public async Task<VerifyEmailResult> VerifyEmail(string userId, string token)
    {
        var account = await _accountVault
            .Where(a => a.StorageId == userId)
            .FirstOrDefaultAsync();

        if (account == null)
            return VerifyEmailResult.RFailure("User not found");

        if (account.EmailVerified)
            return VerifyEmailResult.RSuccess();

        if (string.IsNullOrEmpty(account.EmailVerificationToken) || !string.Equals(account.EmailVerificationToken, token, StringComparison.Ordinal))
            return VerifyEmailResult.RFailure("Invalid token");

        account.EmailVerified = true;
        account.EmailVerificationToken = token;

        await _accountVault.SaveAsync(account);
        return VerifyEmailResult.RSuccess();
    }

    private Uri BuildVerificationLink(string userId, string token)
    {
        var baseUrl = _urls.PublicBaseUrl?.TrimEnd('/') ?? "";
        var uri = $"{baseUrl}/api/signup/verify?uid={Uri.EscapeDataString(userId)}&token={Uri.EscapeDataString(token)}";
        return new Uri(uri);
    }
}



public sealed class SignupResult
{
    public bool Success { get; }
    public string? Error { get; }
    public string? UserId { get; }
    public string? Username { get; }
    public string? Email { get; }
    public bool RequiresEmailVerification { get; }
    public SignupResponse.VerificationInfo? Verification { get; }

    private SignupResult(
        bool success,
        string? error,
        string? userId,
        string? username,
        string? email,
        bool requiresEmailVerification,
        SignupResponse.VerificationInfo? verification)
    {
        Success = success;
        Error = error;
        UserId = userId;
        Username = username;
        Email = email;
        RequiresEmailVerification = requiresEmailVerification;
        Verification = verification;
    }

    public static SignupResult ROk(
        string userId,
        string username,
        string email,
        bool requiresEmailVerification,
        SignupResponse.VerificationInfo? verification)
        => new SignupResult(true, null, userId, username, email, requiresEmailVerification, verification);

    public static SignupResult RFailure(string error)
        => new SignupResult(false, error, null, null, null, false, null);
}

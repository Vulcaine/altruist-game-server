
using Altruist;
using Altruist.Security;
using Altruist.Security.Auth;

using Microsoft.Extensions.Options;

using Server.Email;
using Server.Persistence;
using Server.Signup;

namespace Server;

public interface IVerifyEmail
{
    Task<VerifyEmailResult> VerifyAsync(string uid, string token);
}

[Service(typeof(ILoginService))]
public class LoginService : ILoginService, IVerifyEmail
{
    private readonly IVault<Account> _accountVault;
    private readonly IEmailService _emailService;
    private readonly AppUrls _urls;

    public LoginService(IVault<Account> accountVault, IEmailService emailService, IOptions<AppUrls> urls)
    {
        _accountVault = accountVault;
        _emailService = emailService;
        _urls = urls.Value;
    }

    public Task<LoginResult> LoginAsync(LoginRequest request)
    {
        throw new NotImplementedException();
    }
    public async Task<SignupResult> SignupAsync(SignupRequest request)
    {
        var existingByUsername = await _accountVault
            .Where(acc => acc.Username == request.Username)
            .FirstOrDefaultAsync();

        var existingByEmail = await _accountVault
            .Where(acc => acc.Email == request.Email)
            .FirstOrDefaultAsync();

        if (existingByUsername != null)
        {
            var reason = "Username is already taken";
            return SignupResult.RFailure(reason);
        }

        if (existingByEmail != null)
        {
            var reason = "Email is already in use";
            return SignupResult.RFailure(reason);
        }

        var token = Guid.NewGuid().ToString("N");
        var expiresAt = DateTimeOffset.UtcNow.AddHours(24);

        var account = new Account
        {
            Username = request.Username!,
            PasswordHash = request.Password,
            Email = request.Email!,
            EmailVerified = false,
            EmailVerificationToken = token
        };

        await _accountVault.SaveAsync(account);

        var verifyLink = BuildVerificationLink(account.StorageId, token);
        await _emailService.SendVerificationEmail(request.Email!, request.Username!, verifyLink);

        var verification = new VerificationInfo
        {
            Method = "email",
            SentTo = request.Email!,
            ExpiresAt = expiresAt
        };

        return SignupResult.ROk(
            account,
            true,
            verification
        );
    }

    public async Task<VerifyEmailResult> VerifyAsync(string userId, string token)
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
        var uri = $"{baseUrl}/api/v1/auth/verify?uid={Uri.EscapeDataString(userId)}&token={Uri.EscapeDataString(token)}";
        return new Uri(uri);
    }
}
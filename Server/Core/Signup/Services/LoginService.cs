
using Altruist;
using Altruist.Persistence;
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
    private readonly IPasswordHasher _passwordHasher;
    private readonly AppUrls _urls;

    public LoginService(
        IVault<Account> accountVault,
        IEmailService emailService,
        IPasswordHasher passwordHasher,
        IOptions<AppUrls> urls
    )
    {
        _accountVault = accountVault;
        _emailService = emailService;
        _passwordHasher = passwordHasher;
        _urls = urls.Value;
    }

    public async Task<LoginResult> LoginAsync(LoginRequest request)
    {
        if (!(request is UsernamePasswordLoginRequest usernamePasswordLoginRequest))
        {
            return LoginResult.RFailure("Invalid login request. Method not supported.");
        }

        var account = await _accountVault
             .Where(acc => acc.Username == usernamePasswordLoginRequest.Username
                && acc.PasswordHash == _passwordHasher.Hash(usernamePasswordLoginRequest.Password))
             .FirstAsync();

        if (account != null)
        {
            return LoginResult.ROk(account);
        }

        return LoginResult.RFailure("Invalid username or password");
    }

    [Transactional]
    public async Task<SignupResult> SignupAsync(SignupRequest request)
    {
        var existingByUsername = await _accountVault
            .Where(acc => acc.Username == request.Username || acc.Email == request.Email)
            .FirstOrDefaultAsync();


        if (existingByUsername != null)
        {
            var reason = existingByUsername.Username == request.Username
                ? "Username is already in use"
                : "Email is already in use";
            return SignupResult.RFailure(reason);
        }

        var token = Guid.NewGuid().ToString("N");
        var expiresAt = DateTimeOffset.UtcNow.AddHours(24);

        var account = new Account
        {
            Username = request.Username!,
            PasswordHash = _passwordHasher.Hash(request.Password),
            Email = request.Email!,
            EmailVerified = false,
            EmailVerificationToken = token
        };

        await _accountVault.SaveAsync(account);

        var verifyLink = BuildVerificationLink(account.StorageId, token);
        VerificationInfo? verification = await SendEmailVerification(request.Email, request.Username!, verifyLink);

        return SignupResult.ROk(
            account,
            true,
            verification
        );
    }

    [Transactional]
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

    private async Task<VerificationInfo?> SendEmailVerification(string? email, string username, Uri verifyLink)
    {
        if (email == null)
            return null;

        try
        {
            await _emailService.SendVerificationEmail(email, username, verifyLink);
            return new VerificationInfo
            {
                Method = "email",
                SentTo = email,
                ExpiresAt = DateTimeOffset.UtcNow.AddHours(24)
            };
        }
        catch (Exception)
        {
            return null;
        }
    }
}
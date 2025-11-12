using System.Net;
using System.Net.Mail;
using System.Text;

using Microsoft.Extensions.Options;

namespace Server.Email;

public interface IEmailService
{
    Task SendVerificationEmail(string to, string username, Uri link);
}

public sealed class SmtpOptions
{
    public string Host { get; set; } = "";
    public int Port { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;
    public string From { get; set; } = "";
    public string? User { get; set; }
    public string? Password { get; set; }
}

public sealed class SmtpEmailService : IEmailService
{
    private readonly SmtpOptions _options;

    public SmtpEmailService(IOptions<SmtpOptions> options)
    {
        _options = options.Value;
    }

    public async Task SendVerificationEmail(string to, string username, Uri link)
    {
        using var client = new SmtpClient(_options.Host, _options.Port)
        {
            EnableSsl = _options.EnableSsl,
            Credentials = string.IsNullOrEmpty(_options.User)
                ? CredentialCache.DefaultNetworkCredentials
                : new NetworkCredential(_options.User, _options.Password)
        };

        var subject = "Verify your email";
        var body = new StringBuilder()
            .AppendLine($"Hello {username},")
            .AppendLine()
            .AppendLine("Please verify your email by clicking the link below:")
            .AppendLine(link.ToString())
            .AppendLine()
            .AppendLine("If you did not create this account, you can ignore this email.")
            .ToString();

        using var message = new MailMessage(_options.From, to, subject, body);
        await client.SendMailAsync(message);
    }
}

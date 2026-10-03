using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace StudentNameMVC.Security;

public sealed class PasswordResetEmailOptions
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;
    public string From { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string PublicBaseUrl { get; set; } = string.Empty;
}

public interface IPasswordResetEmailSender
{
    Task SendAsync(string email, string token, CancellationToken ct);
}

public sealed class PasswordResetEmailSender(IOptions<PasswordResetEmailOptions> options) : IPasswordResetEmailSender
{
    public async Task SendAsync(string email, string token, CancellationToken ct)
    {
        var settings = options.Value;
        if (string.IsNullOrWhiteSpace(settings.Host) || string.IsNullOrWhiteSpace(settings.From) ||
            !Uri.TryCreate(settings.PublicBaseUrl, UriKind.Absolute, out var baseUri) ||
            (baseUri.Scheme != "https" && !(baseUri.IsLoopback && baseUri.Scheme == "http")))
            throw new InvalidOperationException("PasswordResetEmail SMTP configuration is required.");
        // Build links from trusted configuration rather than the incoming Host header.
        var link = new Uri(baseUri, "/Account/ResetPassword").AbsoluteUri +
            "?email=" + Uri.EscapeDataString(email) + "&token=" + Uri.EscapeDataString(token);
        using var message = new MailMessage(settings.From, email)
        {
            Subject = "AIVES - Khôi phục mật khẩu",
            Body = "Đặt lại mật khẩu qua liên kết dưới đây (có hiệu lực 20 phút):\n" + link +
                "\nNếu bạn không yêu cầu, hãy bỏ qua email này.",
            IsBodyHtml = false
        };
        using var client = new SmtpClient(settings.Host, settings.Port)
        {
            EnableSsl = settings.EnableSsl,
            Credentials = string.IsNullOrWhiteSpace(settings.Username) ? null :
                new NetworkCredential(settings.Username, settings.Password)
        };
        await client.SendMailAsync(message, ct);
    }
}

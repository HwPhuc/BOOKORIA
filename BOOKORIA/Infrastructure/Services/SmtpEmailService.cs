using System.Net;
using System.Net.Mail;
using BOOKORIA.Application.Abstractions;
using BOOKORIA.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace BOOKORIA.Infrastructure.Services;

public class SmtpEmailService(
    IOptions<EmailOptions> emailOptions,
    ILogger<SmtpEmailService> logger) : IEmailService
{
    public async Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        var options = emailOptions.Value;
        if (string.IsNullOrWhiteSpace(options.SmtpHost))
        {
            logger.LogWarning("SMTP host is not configured. Cannot send email to {To}.", to);
            return;
        }

        using var message = new MailMessage(options.FromAddress, to, subject, body)
        {
            IsBodyHtml = false
        };

        using var client = new SmtpClient(options.SmtpHost, options.SmtpPort)
        {
            EnableSsl = options.EnableSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network
        };

        if (!string.IsNullOrWhiteSpace(options.SmtpUsername))
        {
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential(options.SmtpUsername, options.SmtpPassword);
        }
        else
        {
            client.UseDefaultCredentials = true;
        }

        await client.SendMailAsync(message, cancellationToken);
        logger.LogInformation("Sent email to {To} with subject {Subject}", to, subject);
    }
}

//using System.Net;
//using System.Net.Mail;
//using BOOKORIA.Application.Abstractions;
//using BOOKORIA.Infrastructure.Options;
//using Microsoft.Extensions.Options;

//namespace BOOKORIA.Infrastructure.Services;

//public class SmtpEmailService(
//    IOptions<EmailOptions> emailOptions,
//    ILogger<SmtpEmailService> logger) : IEmailService
//{
//    public async Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
//    {
//        var options = emailOptions.Value;
//        if (string.IsNullOrWhiteSpace(options.SmtpHost))
//        {
//            logger.LogWarning("SMTP host is not configured. Cannot send email to {To}.", to);
//            return;
//        }

//        using var message = new MailMessage(options.FromAddress, to, subject, body)
//        {
//            IsBodyHtml = false
//        };

//        using var client = new SmtpClient(options.SmtpHost, options.SmtpPort)
//        {
//            EnableSsl = options.EnableSsl,
//            DeliveryMethod = SmtpDeliveryMethod.Network
//        };

//        if (!string.IsNullOrWhiteSpace(options.SmtpUsername))
//        {
//            client.UseDefaultCredentials = false;
//            client.Credentials = new NetworkCredential(options.SmtpUsername, options.SmtpPassword);
//        }
//        else
//        {
//            client.UseDefaultCredentials = true;
//        }

//        await client.SendMailAsync(message, cancellationToken);
//        logger.LogInformation("Sent email to {To} with subject {Subject}", to, subject);
//    }
//}






using System.Net;
using System.Net.Mail;
using BOOKORIA.Application.Abstractions;
using BOOKORIA.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace BOOKORIA.Infrastructure.Services;

public class SmtpEmailService(
    IOptions<EmailOptions> emailOptions,
    ILogger<SmtpEmailService> logger)
    : IEmailService
{
    public async Task SendAsync(
        string to,
        string subject,
        string body,
        CancellationToken cancellationToken = default)
    {
        var options = emailOptions.Value;

        logger.LogInformation(
            """
            SMTP Config:
            Host={Host}
            Port={Port}
            EnableSsl={EnableSsl}
            Username={Username}
            From={From}
            HasPassword={HasPassword}
            """,
            options.SmtpHost,
            options.SmtpPort,
            options.EnableSsl,
            options.SmtpUsername,
            options.FromAddress,
            !string.IsNullOrWhiteSpace(options.SmtpPassword));

        if (string.IsNullOrWhiteSpace(options.SmtpHost))
        {
            logger.LogError("SMTP Host is empty");

            throw new InvalidOperationException(
                "SMTP Host is not configured.");
        }

        try
        {
            using var message = new MailMessage(
                options.FromAddress,
                to,
                subject,
                body)
            {
                IsBodyHtml = false
            };

            using var client = new SmtpClient(
                options.SmtpHost,
                options.SmtpPort)
            {
                EnableSsl = options.EnableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Timeout = 30000
            };

            client.UseDefaultCredentials = false;

            client.Credentials =
                new NetworkCredential(
                    options.SmtpUsername,
                    options.SmtpPassword);

            logger.LogInformation(
                "Connecting SMTP server...");

            await client.SendMailAsync(
                message,
                cancellationToken);

            logger.LogInformation(
                "Email sent successfully to {To}",
                to);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "SMTP send failed. To={To}",
                to);

            throw;
        }
    }
}
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






using BOOKORIA.Application.Abstractions;
using BOOKORIA.Infrastructure.Options;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace BOOKORIA.Infrastructure.Services;

public class SmtpEmailService(
    IOptions<EmailOptions> emailOptions,
    ILogger<SmtpEmailService> logger) : IEmailService
{
    public async Task SendAsync(
        string to,
        string subject,
        string body,
        CancellationToken cancellationToken = default)
    {
        var options = emailOptions.Value;

        logger.LogInformation(
            "SMTP Config: Host={Host}, Port={Port}, EnableSsl={EnableSsl}, Username={Username}, From={From}, HasPassword={HasPassword}",
            options.SmtpHost,
            options.SmtpPort,
            options.EnableSsl,
            options.SmtpUsername,
            options.FromAddress,
            !string.IsNullOrWhiteSpace(options.SmtpPassword));

        if (string.IsNullOrWhiteSpace(options.SmtpHost))
        {
            logger.LogWarning("SMTP host is not configured. Cannot send email to {To}.", to);
            return;
        }

        var message = new MimeMessage();

        message.From.Add(MailboxAddress.Parse(options.FromAddress));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;

        message.Body = new TextPart("plain")
        {
            Text = body
        };

        using var smtp = new SmtpClient();

        smtp.Timeout = 60000;

        try
        {
            var secureSocketOptions = options.SmtpPort == 465
                ? SecureSocketOptions.SslOnConnect
                : SecureSocketOptions.StartTls;

            logger.LogInformation(
                "Connecting to SMTP server {Host}:{Port} using {SecureSocketOptions}",
                options.SmtpHost,
                options.SmtpPort,
                secureSocketOptions);

            await smtp.ConnectAsync(
                options.SmtpHost,
                options.SmtpPort,
                secureSocketOptions,
                cancellationToken);

            logger.LogInformation("SMTP connected");

            if (!string.IsNullOrWhiteSpace(options.SmtpUsername))
            {
                await smtp.AuthenticateAsync(
                    options.SmtpUsername,
                    options.SmtpPassword,
                    cancellationToken);

                logger.LogInformation("SMTP authenticated");
            }

            await smtp.SendAsync(message, cancellationToken);

            logger.LogInformation("Email sent successfully to {To}", to);

            await smtp.DisconnectAsync(true, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "SMTP send failed. To={To}", to);
            throw;
        }
    }
}
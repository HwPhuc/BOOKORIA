using BOOKORIA.Application.Abstractions;
using Microsoft.Extensions.Options;
using BOOKORIA.Infrastructure.Options;

namespace BOOKORIA.Infrastructure.Services;

public class LoggingEmailService(
    ILogger<LoggingEmailService> logger,
    IOptions<EmailOptions> emailOptions) : IEmailService
{
    public Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "[EMAIL] From: {From}; To: {To}; Subject: {Subject}; Body: {Body}",
            emailOptions.Value.FromAddress,
            to,
            subject,
            body);

        return Task.CompletedTask;
    }
}

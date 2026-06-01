namespace BOOKORIA.Application.Abstractions;

public interface IStripeWebhookService
{
    Task HandleCheckoutCompletedAsync(string stripeSessionId, CancellationToken cancellationToken = default);
}

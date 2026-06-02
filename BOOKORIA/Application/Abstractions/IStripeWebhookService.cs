//namespace BOOKORIA.Application.Abstractions;

//public interface IStripeWebhookService
//{
//    Task HandleCheckoutCompletedAsync(string stripeSessionId, CancellationToken cancellationToken = default);
//}



namespace BOOKORIA.Application.Abstractions;

public interface IStripeWebhookService
{
    Task HandleCheckoutCompletedAsync(
        string stripeSessionId,
        bool sendEmail = true,
        CancellationToken cancellationToken = default);
}
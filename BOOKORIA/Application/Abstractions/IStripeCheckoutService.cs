namespace BOOKORIA.Application.Abstractions;

public interface IStripeCheckoutService
{
    Task<string> CreateCheckoutUrlAsync(
        Guid orderId,
        IReadOnlyCollection<StripeCheckoutLineItem> items,
        decimal totalAmount,
        string successUrl,
        string cancelUrl,
        CancellationToken cancellationToken = default);

    Task<bool> CompleteCheckoutAsync(string stripeSessionId, CancellationToken cancellationToken = default);
}

public sealed record StripeCheckoutLineItem(string Name, decimal UnitPrice, long Quantity);

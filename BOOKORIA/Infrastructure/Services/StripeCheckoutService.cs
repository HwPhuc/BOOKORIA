//using BOOKORIA.Application.Abstractions;
//using BOOKORIA.Domain.Entities;
//using BOOKORIA.Domain.Enums;
//using BOOKORIA.Infrastructure.Data;
//using BOOKORIA.Infrastructure.Options;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Options;
//using Stripe;
//using Stripe.Checkout;

//namespace BOOKORIA.Infrastructure.Services;

//public class StripeCheckoutService(
//    BookoriaDbContext dbContext,
//    IStripeWebhookService stripeWebhookService,
//    IOptions<StripeOptions> options,
//    ILogger<StripeCheckoutService> logger) : IStripeCheckoutService
//{
//    public async Task<string> CreateCheckoutUrlAsync(
//        Guid orderId,
//        IReadOnlyCollection<StripeCheckoutLineItem> items,
//        decimal totalAmount,
//        string successUrl,
//        string cancelUrl,
//        CancellationToken cancellationToken = default)
//    {
//        if (string.IsNullOrWhiteSpace(options.Value.SecretKey))
//        {
//            throw new InvalidOperationException("Stripe SecretKey is not configured.");
//        }

//        StripeConfiguration.ApiKey = options.Value.SecretKey;

//        var sessionService = new SessionService();
//        var session = await sessionService.CreateAsync(new SessionCreateOptions
//        {
//            Mode = "payment",
//            SuccessUrl = successUrl,
//            CancelUrl = cancelUrl,
//            PaymentMethodTypes = ["card"],
//            ClientReferenceId = orderId.ToString(),
//            LineItems = items.Select(x => new SessionLineItemOptions
//            {
//                Quantity = x.Quantity,
//                PriceData = new SessionLineItemPriceDataOptions
//                {
//                    Currency = options.Value.Currency,
//                    UnitAmount = ToStripeAmount(x.UnitPrice),
//                    ProductData = new SessionLineItemPriceDataProductDataOptions
//                    {
//                        Name = x.Name
//                    }
//                }
//            }).ToList()
//        }, cancellationToken: cancellationToken);

//        if (string.IsNullOrWhiteSpace(session.Id) || string.IsNullOrWhiteSpace(session.Url))
//        {
//            throw new InvalidOperationException("Cannot create Stripe checkout session.");
//        }

//        var existingPayment = await dbContext.Payments
//            .FirstOrDefaultAsync(x => x.OrderId == orderId, cancellationToken);

//        if (existingPayment is null)
//        {
//            dbContext.Payments.Add(new Payment
//            {
//                Id = Guid.NewGuid(),
//                OrderId = orderId,
//                StripeSessionId = session.Id,
//                Amount = totalAmount,
//                Status = PaymentStatus.Pending
//            });
//        }
//        else
//        {
//            existingPayment.StripeSessionId = session.Id;
//            existingPayment.Amount = totalAmount;
//            existingPayment.Status = PaymentStatus.Pending;
//        }

//        await dbContext.SaveChangesAsync(cancellationToken);

//        return session.Url;
//    }

//    public async Task<bool> CompleteCheckoutAsync(string stripeSessionId, CancellationToken cancellationToken = default)
//    {
//        if (string.IsNullOrWhiteSpace(stripeSessionId) || !stripeSessionId.StartsWith("cs_", StringComparison.OrdinalIgnoreCase))
//        {
//            logger.LogWarning("Invalid Stripe session id received: {StripeSessionId}", stripeSessionId);
//            return false;
//        }

//        if (string.IsNullOrWhiteSpace(options.Value.SecretKey))
//        {
//            throw new InvalidOperationException("Stripe SecretKey is not configured.");
//        }

//        StripeConfiguration.ApiKey = options.Value.SecretKey;

//        var sessionService = new SessionService();

//        Session session;
//        try
//        {
//            session = await sessionService.GetAsync(stripeSessionId, cancellationToken: cancellationToken);
//        }
//        catch (StripeException ex)
//        {
//            logger.LogWarning(ex, "Cannot get Stripe session {StripeSessionId}", stripeSessionId);
//            return false;
//        }

//        if (!string.Equals(session.PaymentStatus, "paid", StringComparison.OrdinalIgnoreCase))
//        {
//            logger.LogInformation("Stripe session {SessionId} is not paid yet", stripeSessionId);
//            return false;
//        }

//        await stripeWebhookService.HandleCheckoutCompletedAsync(stripeSessionId, cancellationToken);
//        return true;
//    }

//    private static long ToStripeAmount(decimal amount)
//    {
//        return (long)Math.Round(amount, MidpointRounding.AwayFromZero);
//    }
//}





using BOOKORIA.Application.Abstractions;
using BOOKORIA.Domain.Entities;
using BOOKORIA.Domain.Enums;
using BOOKORIA.Infrastructure.Data;
using BOOKORIA.Infrastructure.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;

namespace BOOKORIA.Infrastructure.Services;

public class StripeCheckoutService(
    BookoriaDbContext dbContext,
    IStripeWebhookService stripeWebhookService,
    IOptions<StripeOptions> options,
    ILogger<StripeCheckoutService> logger) : IStripeCheckoutService
{
    public async Task<string> CreateCheckoutUrlAsync(
        Guid orderId,
        IReadOnlyCollection<StripeCheckoutLineItem> items,
        decimal totalAmount,
        string successUrl,
        string cancelUrl,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(options.Value.SecretKey))
            throw new InvalidOperationException("Stripe SecretKey is not configured.");

        StripeConfiguration.ApiKey = options.Value.SecretKey;

        var sessionService = new SessionService();

        var session = await sessionService.CreateAsync(new SessionCreateOptions
        {
            Mode = "payment",
            SuccessUrl = successUrl,
            CancelUrl = cancelUrl,
            PaymentMethodTypes = ["card"],
            ClientReferenceId = orderId.ToString(),
            LineItems = items.Select(x => new SessionLineItemOptions
            {
                Quantity = x.Quantity,
                PriceData = new SessionLineItemPriceDataOptions
                {
                    Currency = options.Value.Currency,
                    UnitAmount = ToStripeAmount(x.UnitPrice),
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = x.Name
                    }
                }
            }).ToList()
        }, cancellationToken: cancellationToken);

        if (string.IsNullOrWhiteSpace(session.Id) || string.IsNullOrWhiteSpace(session.Url))
            throw new InvalidOperationException("Cannot create Stripe checkout session.");

        var existingPayment = await dbContext.Payments
            .FirstOrDefaultAsync(x => x.OrderId == orderId, cancellationToken);

        if (existingPayment is null)
        {
            dbContext.Payments.Add(new Payment
            {
                Id = Guid.NewGuid(),
                OrderId = orderId,
                StripeSessionId = session.Id,
                Amount = totalAmount,
                Status = PaymentStatus.Pending
            });
        }
        else
        {
            existingPayment.StripeSessionId = session.Id;
            existingPayment.Amount = totalAmount;
            existingPayment.Status = PaymentStatus.Pending;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return session.Url;
    }

    public async Task<bool> CompleteCheckoutAsync(
        string stripeSessionId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(stripeSessionId) ||
            !stripeSessionId.StartsWith("cs_", StringComparison.OrdinalIgnoreCase))
        {
            logger.LogWarning("Invalid Stripe session id received: {StripeSessionId}", stripeSessionId);
            return false;
        }

        if (string.IsNullOrWhiteSpace(options.Value.SecretKey))
            throw new InvalidOperationException("Stripe SecretKey is not configured.");

        StripeConfiguration.ApiKey = options.Value.SecretKey;

        var sessionService = new SessionService();

        Session session;

        try
        {
            session = await sessionService.GetAsync(
                stripeSessionId,
                cancellationToken: cancellationToken);
        }
        catch (StripeException ex)
        {
            logger.LogWarning(ex, "Cannot get Stripe session {StripeSessionId}", stripeSessionId);
            return false;
        }

        if (!string.Equals(session.PaymentStatus, "paid", StringComparison.OrdinalIgnoreCase))
        {
            logger.LogInformation("Stripe session {StripeSessionId} is not paid yet", stripeSessionId);
            return false;
        }

        await stripeWebhookService.HandleCheckoutCompletedAsync(
            stripeSessionId,
            sendEmail: true,
            cancellationToken);

        logger.LogInformation(
            "Stripe session {StripeSessionId} is paid. Order processed without email in redirect flow.",
            stripeSessionId);

        return true;
    }

    private static long ToStripeAmount(decimal amount)
    {
        return (long)Math.Round(amount, MidpointRounding.AwayFromZero);
    }
}
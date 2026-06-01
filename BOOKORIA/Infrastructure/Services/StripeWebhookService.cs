using BOOKORIA.Application.Abstractions;
using BOOKORIA.Domain.Entities;
using BOOKORIA.Domain.Enums;
using BOOKORIA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BOOKORIA.Infrastructure.Services;

public class StripeWebhookService(
    BookoriaDbContext dbContext,
    IServiceProvider serviceProvider, //
    IEbookDeliveryService ebookDeliveryService,
    ILogger<StripeWebhookService> logger) : IStripeWebhookService
{
    public async Task HandleCheckoutCompletedAsync(string stripeSessionId, CancellationToken cancellationToken = default)
    {
        var payment = await dbContext.Payments
            .Include(x => x.Order)
            .ThenInclude(x => x.Items)
            .ThenInclude(x => x.Book)
            .FirstOrDefaultAsync(x => x.StripeSessionId == stripeSessionId, cancellationToken);

        if (payment is null)
        {
            logger.LogWarning("No payment found for Stripe session {StripeSessionId}", stripeSessionId);
            return;
        }

        if (payment.Status == PaymentStatus.Succeeded)
        {
            logger.LogInformation("Stripe session {StripeSessionId} already processed", stripeSessionId);
            //
            if (payment.Order.Items.Any(x => x.ItemType == "Ebook"))
            {
                // Gọi gửi bù ngầm bằng cách tạo Scope mới độc lập hoàn toàn
                var orderId = payment.OrderId;
                _ = Task.Run(async () =>
                {
                    try
                    {
                        using var scope = serviceProvider.CreateScope();
                        var scopedDeliveryService = scope.ServiceProvider.GetRequiredService<IEbookDeliveryService>();
                        await scopedDeliveryService.SendEbookAsync(orderId, CancellationToken.None);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Lỗi gửi mail bù ngầm thất bại.");
                    }
                });
            }
            //
            return;
        }

        payment.Status = PaymentStatus.Succeeded;
        payment.Order.PaymentStatus = PaymentStatus.Succeeded;
        payment.Order.Status = OrderStatus.Paid;

        var hasPhysicalItems = payment.Order.Items.Any(x => x.ItemType == "PhysicalBook");
        var hasEbookItems = payment.Order.Items.Any(x => x.ItemType == "Ebook");

        if (hasPhysicalItems)
        {
            foreach (var item in payment.Order.Items.Where(x => x.ItemType == "PhysicalBook"))
            {
                item.Book.Stock = Math.Max(0, item.Book.Stock - item.Quantity);
            }

            var hasShipment = await dbContext.Shipments
                .AnyAsync(x => x.OrderId == payment.OrderId, cancellationToken);

            if (!hasShipment)
            {
                var shipmentId = Guid.NewGuid();
                dbContext.Shipments.Add(new Shipment
                {
                    Id = shipmentId,
                    OrderId = payment.OrderId,
                    ShippingStatus = ShippingStatus.WaitingPickup,
                    LastUpdatedAtUtc = DateTime.UtcNow,
                    Trackings =
                    {
                        new ShipmentTracking
                        {
                            Id = Guid.NewGuid(),
                            ShipmentId = shipmentId,
                            Status = ShippingStatus.WaitingPickup,
                            Note = "Đơn hàng đã thanh toán và đang chờ bàn giao cho đơn vị vận chuyển.",
                            TimestampUtc = DateTime.UtcNow
                        }
                    }
                });
            }
        }

        if (hasEbookItems)
        {
            var existingDelivery = await dbContext.EbookDeliveries
                .AnyAsync(x => x.OrderId == payment.OrderId, cancellationToken);

            if (!existingDelivery)
            {
                var userEmail = await dbContext.Users
                    .Where(x => x.Id == payment.Order.UserId)
                    .Select(x => x.Email)
                    .FirstOrDefaultAsync(cancellationToken);

                dbContext.EbookDeliveries.Add(new EbookDelivery
                {
                    Id = Guid.NewGuid(),
                    OrderId = payment.OrderId,
                    EmailTo = userEmail ?? "customer@example.com",
                    DownloadToken = Guid.NewGuid().ToString("N"),
                    ExpiredAtUtc = DateTime.UtcNow.AddDays(3)
                });
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        if (hasEbookItems)
        {
            await ebookDeliveryService.SendEbookAsync(payment.OrderId, cancellationToken);
        }
    }
}

using BOOKORIA.Application.Abstractions;
using BOOKORIA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BOOKORIA.Infrastructure.Services;

public class EbookDeliveryService(
    BookoriaDbContext dbContext,
    IEmailService emailService,
    ILogger<EbookDeliveryService> logger) : IEbookDeliveryService
{
    public async Task SendEbookAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var delivery = await dbContext.EbookDeliveries
            .Include(x => x.Order)
            .ThenInclude(x => x.Items)
            .ThenInclude(x => x.Book)
            .FirstOrDefaultAsync(x => x.OrderId == orderId, cancellationToken);

        if (delivery is null)
        {
            logger.LogWarning("Ebook delivery not found for order {OrderId}", orderId);
            return;
        }

        var ebookLines = delivery.Order.Items
            .Where(x => x.ItemType == "Ebook" && !string.IsNullOrWhiteSpace(x.Book.FullPdfUrl))
            .Select(x => $"- {x.Book.Title}: {x.Book.FullPdfUrl}")
            .Distinct()
            .ToList();

        var body = ebookLines.Count > 0
            ? $"Cảm ơn bạn đã mua ebook tại BOOKORIA.\n\nDanh sách file PDF:\n{string.Join("\n", ebookLines)}"
            : "Cảm ơn bạn đã mua ebook tại BOOKORIA. Chúng tôi sẽ cập nhật file PDF sớm nhất qua email này.";

        await emailService.SendAsync(delivery.EmailTo, "BOOKORIA - Ebook download", body, cancellationToken);

        delivery.SentAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}

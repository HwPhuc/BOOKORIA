using BOOKORIA.Domain.Enums;
using BOOKORIA.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BOOKORIA.Pages.Orders;

[Authorize(Policy = "CustomerOnly")]
public class HistoryModel(BookoriaDbContext dbContext) : PageModel
{
    public IReadOnlyList<OrderHistoryItem> Orders { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(userId))
        {
            return;
        }

        Orders = await dbContext.Orders
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new OrderHistoryItem(
                x.Id,
                x.CreatedAtUtc,
                x.OrderType,
                x.Status,
                x.PaymentStatus,
                x.TotalAmount,
                x.Shipment == null ? null : new ShipmentInfo(
                    x.Shipment.ShippingStatus,
                    x.Shipment.TrackingCode,
                    x.Shipment.LastUpdatedAtUtc),
                x.Items.Select(i => new OrderHistoryLine(
                    i.Book.Title,
                    i.ItemType,
                    i.Quantity,
                    i.UnitPrice)).ToList()))
            .ToListAsync(cancellationToken);
    }

    public sealed record OrderHistoryItem(
        Guid Id,
        DateTime CreatedAtUtc,
        OrderType OrderType,
        OrderStatus Status,
        PaymentStatus PaymentStatus,
        decimal TotalAmount,
        ShipmentInfo? Shipment,
        IReadOnlyList<OrderHistoryLine> Lines);

    public sealed record ShipmentInfo(
        ShippingStatus Status,
        string? TrackingCode,
        DateTime LastUpdatedAtUtc);

    public sealed record OrderHistoryLine(
        string BookTitle,
        string ItemType,
        int Quantity,
        decimal UnitPrice);
}

using BOOKORIA.Domain.Enums;
using BOOKORIA.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BOOKORIA.Pages.Admin.Orders;

public class IndexModel(BookoriaDbContext dbContext) : PageModel
{
    public IReadOnlyList<OrderRow> Orders { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Orders = await dbContext.Orders
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new OrderRow(
                x.Id,
                x.CreatedAtUtc,
                dbContext.Users
                    .Where(u => u.Id == x.UserId)
                    .Select(u => u.Email)
                    .FirstOrDefault() ?? x.UserId,
                x.OrderType,
                x.Status,
                x.PaymentStatus,
                x.TotalAmount,
                x.Shipment == null ? null : x.Shipment.ShippingStatus))
            .ToListAsync(cancellationToken);
    }

    public sealed record OrderRow(
        Guid Id,
        DateTime CreatedAtUtc,
        string Customer,
        OrderType OrderType,
        OrderStatus Status,
        PaymentStatus PaymentStatus,
        decimal TotalAmount,
        ShippingStatus? ShippingStatus);
}

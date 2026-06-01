using System.ComponentModel.DataAnnotations;
using BOOKORIA.Domain.Entities;
using BOOKORIA.Domain.Enums;
using BOOKORIA.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BOOKORIA.Pages.Admin.Orders;

public class EditModel(BookoriaDbContext dbContext) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    [TempData]
    public string? Message { get; set; }

    public bool IsPhysicalOrder { get; private set; }

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        var order = await LoadOrderSnapshotAsync(id, cancellationToken);

        if (order is null)
        {
            return NotFound();
        }

        ApplyInputFromOrder(order);

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        var order = await dbContext.Orders
            .AsNoTracking()
            .Include(x => x.Shipment)
            .FirstOrDefaultAsync(x => x.Id == Input.Id, cancellationToken);

        if (order is null)
        {
            return NotFound();
        }

        IsPhysicalOrder = order.OrderType == OrderType.PhysicalBook;

        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (IsPhysicalOrder && !Input.ShippingStatus.HasValue)
        {
            ModelState.AddModelError(nameof(Input.ShippingStatus), "Vui lòng chọn trạng thái vận chuyển.");
            return Page();
        }

        var now = DateTime.UtcNow;

        await dbContext.Orders
            .Where(x => x.Id == Input.Id)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.Status, Input.OrderStatus), cancellationToken);

        if (IsPhysicalOrder)
        {
            var note = string.IsNullOrWhiteSpace(Input.Note)
                ? "Admin cập nhật trạng thái đơn hàng."
                : Input.Note.Trim();

            var carrier = string.IsNullOrWhiteSpace(Input.Carrier) ? null : Input.Carrier.Trim();
            var trackingCode = string.IsNullOrWhiteSpace(Input.TrackingCode) ? null : Input.TrackingCode.Trim();

            if (order.Shipment is null)
            {
                var shipmentId = Guid.NewGuid();
                dbContext.Shipments.Add(new Shipment
                {
                    Id = shipmentId,
                    OrderId = order.Id,
                    ShippingStatus = Input.ShippingStatus!.Value,
                    Carrier = carrier,
                    TrackingCode = trackingCode,
                    LastUpdatedAtUtc = now
                });

                dbContext.ShipmentTrackings.Add(new ShipmentTracking
                {
                    Id = Guid.NewGuid(),
                    ShipmentId = shipmentId,
                    Status = Input.ShippingStatus.Value,
                    Note = note,
                    TimestampUtc = now
                });

                await dbContext.SaveChangesAsync(cancellationToken);
            }
            else
            {
                var previousStatus = order.Shipment.ShippingStatus;

                await dbContext.Shipments
                    .Where(x => x.Id == order.Shipment.Id)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(x => x.ShippingStatus, Input.ShippingStatus!.Value)
                        .SetProperty(x => x.Carrier, carrier)
                        .SetProperty(x => x.TrackingCode, trackingCode)
                        .SetProperty(x => x.LastUpdatedAtUtc, now), cancellationToken);

                if (previousStatus != Input.ShippingStatus.Value || !string.IsNullOrWhiteSpace(Input.Note))
                {
                    dbContext.ShipmentTrackings.Add(new ShipmentTracking
                    {
                        Id = Guid.NewGuid(),
                        ShipmentId = order.Shipment.Id,
                        Status = Input.ShippingStatus.Value,
                        Note = note,
                        TimestampUtc = now
                    });

                    await dbContext.SaveChangesAsync(cancellationToken);
                }
            }
        }

        Message = "Cập nhật trạng thái đơn hàng thành công.";

        return RedirectToPage(new { id = Input.Id });
    }

    public sealed class InputModel
    {
        public Guid Id { get; set; }

        [Display(Name = "Trạng thái đơn hàng")]
        public OrderStatus OrderStatus { get; set; }

        [Display(Name = "Trạng thái vận chuyển")]
        public ShippingStatus? ShippingStatus { get; set; }

        [MaxLength(150)]
        [Display(Name = "Đơn vị vận chuyển")]
        public string? Carrier { get; set; }

        [MaxLength(100)]
        [Display(Name = "Mã vận đơn")]
        public string? TrackingCode { get; set; }

        [MaxLength(500)]
        [Display(Name = "Ghi chú cập nhật")]
        public string? Note { get; set; }
    }

    private async Task<Order?> LoadOrderSnapshotAsync(Guid id, CancellationToken cancellationToken)
    {
        return await dbContext.Orders
            .AsNoTracking()
            .Include(x => x.Shipment)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    private void ApplyInputFromOrder(Order order)
    {
        IsPhysicalOrder = order.OrderType == OrderType.PhysicalBook;

        Input = new InputModel
        {
            Id = order.Id,
            OrderStatus = order.Status,
            ShippingStatus = order.Shipment?.ShippingStatus,
            Carrier = order.Shipment?.Carrier,
            TrackingCode = order.Shipment?.TrackingCode
        };
    }

}

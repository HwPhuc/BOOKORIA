using BOOKORIA.Domain.Enums;

namespace BOOKORIA.Domain.Entities;

public class ShipmentTracking
{
    public Guid Id { get; set; }
    public Guid ShipmentId { get; set; }
    public Shipment Shipment { get; set; } = null!;

    public ShippingStatus Status { get; set; }
    public string? Note { get; set; }
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
}

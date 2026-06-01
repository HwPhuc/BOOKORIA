using BOOKORIA.Domain.Enums;

namespace BOOKORIA.Domain.Entities;

public class Shipment
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public string? Carrier { get; set; }
    public string? TrackingCode { get; set; }
    public ShippingStatus ShippingStatus { get; set; } = ShippingStatus.NotCreated;
    public DateTime LastUpdatedAtUtc { get; set; } = DateTime.UtcNow;
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public ICollection<ShipmentTracking> Trackings { get; set; } = new List<ShipmentTracking>();
}

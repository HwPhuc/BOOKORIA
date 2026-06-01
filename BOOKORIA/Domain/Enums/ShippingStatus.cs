namespace BOOKORIA.Domain.Enums;

public enum ShippingStatus
{
    NotCreated = 1,
    WaitingPickup = 2,
    InTransit = 3,
    Delivered = 4,
    Failed = 5,
    Returned = 6
}

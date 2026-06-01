namespace BOOKORIA.Application.Abstractions;

public interface IEbookDeliveryService
{
    Task SendEbookAsync(Guid orderId, CancellationToken cancellationToken = default);
}

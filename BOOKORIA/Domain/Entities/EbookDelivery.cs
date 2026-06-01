namespace BOOKORIA.Domain.Entities;

public class EbookDelivery
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public string EmailTo { get; set; } = string.Empty;
    public DateTime? SentAtUtc { get; set; }
    public string DownloadToken { get; set; } = string.Empty;
    public DateTime ExpiredAtUtc { get; set; }
}

namespace BOOKORIA.Web.Contracts;

public class StripeWebhookRequest
{
    public string EventType { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
}

using BOOKORIA.Application.Abstractions;
using BOOKORIA.Web.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace BOOKORIA.Controllers;

[ApiController]
[Route("api/webhooks/stripe")]
public class StripeWebhookController(IStripeWebhookService stripeWebhookService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> PostAsync([FromBody] StripeWebhookRequest request, CancellationToken cancellationToken)
    {
        if (!string.Equals(request.EventType, "checkout.session.completed", StringComparison.OrdinalIgnoreCase))
        {
            return Ok(new { Message = "Event ignored" });
        }

        if (string.IsNullOrWhiteSpace(request.SessionId))
        {
            return BadRequest(new { Message = "SessionId is required" });
        }

        await stripeWebhookService.HandleCheckoutCompletedAsync(request.SessionId, cancellationToken);
        return Ok(new { Message = "Processed" });
    }
}

using BOOKORIA.Application.Abstractions;
using BOOKORIA.Web.Cart;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BOOKORIA.Pages.Cart;

[Authorize(Policy = "CustomerOnly")]
public class CheckoutSuccessModel(IStripeCheckoutService stripeCheckoutService) : PageModel
{
    public bool IsPaid { get; private set; }

    [TempData]
    public string? Message { get; set; }

    public async Task<IActionResult> OnGetAsync(string? session_id, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(session_id))
        {
            Message = "Thiếu thông tin phiên thanh toán Stripe.";
            return RedirectToPage("/Cart/Index");
        }

        IsPaid = await stripeCheckoutService.CompleteCheckoutAsync(session_id, cancellationToken);

        if (IsPaid)
        {
            HttpContext.Session.ClearCart();
        }
        else
        {
            Message = "Không xác minh được thanh toán Stripe. Vui lòng kiểm tra lại giao dịch.";
        }

        return Page();
    }
}

using BOOKORIA.Domain.Entities;
using BOOKORIA.Domain.Enums;
using BOOKORIA.Application.Abstractions;
using BOOKORIA.Infrastructure.Data;
using BOOKORIA.Web.Cart;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BOOKORIA.Pages.Cart;

[Authorize(Policy = "CustomerOnly")]
public class IndexModel(
    BookoriaDbContext dbContext,
    IStripeCheckoutService stripeCheckoutService) : PageModel
{
    public IReadOnlyList<CartItemViewModel> Items { get; private set; } = [];
    public decimal TotalAmount { get; private set; }

    [TempData]
    public string? Message { get; set; }

    [BindProperty(SupportsGet = true)]
    public bool Cancelled { get; set; }

    public void OnGet()
    {
        if (Cancelled)
        {
            Message = "Bạn đã hủy thanh toán Stripe.";
        }

        LoadCart();
    }

    public async Task<IActionResult> OnPostUpdateQuantityAsync(Guid bookId, string itemType, int quantity, CancellationToken cancellationToken)
    {
        var cart = HttpContext.Session.GetCart();
        var item = cart.FirstOrDefault(x => x.BookId == bookId && x.ItemType == itemType);
        if (item is null)
        {
            return RedirectToPage();
        }

        quantity = Math.Max(1, quantity);

        if (string.Equals(itemType, "PhysicalBook", StringComparison.OrdinalIgnoreCase))
        {
            var stock = await dbContext.Books
                .AsNoTracking()
                .Where(x => x.Id == bookId)
                .Select(x => x.Stock)
                .FirstOrDefaultAsync(cancellationToken);

            quantity = Math.Min(quantity, Math.Max(stock, 1));
        }

        item.Quantity = quantity;
        HttpContext.Session.SaveCart(cart);

        return RedirectToPage();
    }

    public IActionResult OnPostRemove(Guid bookId, string itemType)
    {
        var cart = HttpContext.Session.GetCart();
        cart.RemoveAll(x => x.BookId == bookId && x.ItemType == itemType);
        HttpContext.Session.SaveCart(cart);

        return RedirectToPage();
    }

    public IActionResult OnPostClear()
    {
        HttpContext.Session.ClearCart();
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostCheckoutAsync(CancellationToken cancellationToken)
    {
        var cart = HttpContext.Session.GetCart();
        if (cart.Count == 0)
        {
            return RedirectToPage();
        }

        var bookIds = cart.Select(x => x.BookId).Distinct().ToList();
        var books = await dbContext.Books
            .Where(x => bookIds.Contains(x.Id) && x.IsActive)
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        foreach (var item in cart)
        {
            if (!books.TryGetValue(item.BookId, out var book))
            {
                Message = "Có sách không hợp lệ trong giỏ hàng. Vui lòng kiểm tra lại.";
                return RedirectToPage();
            }

            if (item.ItemType == "PhysicalBook" && item.Quantity > book.Stock)
            {
                Message = $"Sách '{book.Title}' chỉ còn {book.Stock} cuốn.";
                return RedirectToPage();
            }
        }

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        var orderType = cart.Any(x => x.ItemType == "PhysicalBook")
            ? OrderType.PhysicalBook
            : OrderType.Ebook;

        var order = new Order
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            OrderType = orderType,
            Status = OrderStatus.Pending,
            PaymentStatus = PaymentStatus.Pending,
            TotalAmount = 0m
        };

        foreach (var item in cart)
        {
            var book = books[item.BookId];
            var unitPrice = item.ItemType == "PhysicalBook" ? book.PricePrint : book.PriceEbook;
            order.TotalAmount += unitPrice * item.Quantity;

            order.Items.Add(new OrderItem
            {
                Id = Guid.NewGuid(),
                BookId = book.Id,
                Quantity = item.Quantity,
                UnitPrice = unitPrice,
                ItemType = item.ItemType
            });
        }

        dbContext.Orders.Add(order);
        await dbContext.SaveChangesAsync(cancellationToken);

        var lineItems = cart.Select(x => new StripeCheckoutLineItem(
            Name: $"{x.Title} ({(x.ItemType == "PhysicalBook" ? "Sách giấy" : "Ebook")})",
            UnitPrice: x.UnitPrice,
            Quantity: x.Quantity)).ToList();

        var checkoutSuccessPath = Url.Page("/Cart/CheckoutSuccess");
        var cartIndexPath = Url.Page("/Cart/Index");

        var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
        var successUrl = $"{baseUrl}{checkoutSuccessPath}?session_id={{CHECKOUT_SESSION_ID}}";
        var cancelUrl = $"{baseUrl}{cartIndexPath}?cancelled=true";

        if (string.IsNullOrWhiteSpace(successUrl) || string.IsNullOrWhiteSpace(cancelUrl))
        {
            Message = "Không thể tạo URL thanh toán. Vui lòng thử lại.";
            return RedirectToPage();
        }

        var stripeCheckoutUrl = await stripeCheckoutService.CreateCheckoutUrlAsync(
            order.Id,
            lineItems,
            order.TotalAmount,
            successUrl,
            cancelUrl,
            cancellationToken);

        return Redirect(stripeCheckoutUrl);
    }

    private void LoadCart()
    {
        var cart = HttpContext.Session.GetCart();

        Items = cart
            .OrderBy(x => x.Title)
            .Select(x => new CartItemViewModel(
                x.BookId,
                x.Title,
                x.Author,
                x.CoverUrl,
                x.ItemType,
                x.UnitPrice,
                x.Quantity,
                x.UnitPrice * x.Quantity))
            .ToList();

        TotalAmount = Items.Sum(x => x.LineTotal);
    }

    public sealed record CartItemViewModel(
        Guid BookId,
        string Title,
        string Author,
        string? CoverUrl,
        string ItemType,
        decimal UnitPrice,
        int Quantity,
        decimal LineTotal);
}

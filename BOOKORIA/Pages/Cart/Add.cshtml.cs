using BOOKORIA.Infrastructure.Data;
using BOOKORIA.Web.Cart;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BOOKORIA.Pages.Cart;

[Authorize(Policy = "CustomerOnly")]
public class AddModel(BookoriaDbContext dbContext) : PageModel
{
    public async Task<IActionResult> OnPostAsync(
        Guid bookId,
        string itemType,
        int quantity = 1,
        string? returnUrl = null,
        CancellationToken cancellationToken = default)
    {
        if (quantity < 1)
        {
            quantity = 1;
        }

        var normalizedType = string.Equals(itemType, "PhysicalBook", StringComparison.OrdinalIgnoreCase)
            ? "PhysicalBook"
            : "Ebook";

        var book = await dbContext.Books
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == bookId && x.IsActive, cancellationToken);

        if (book is null)
        {
            return RedirectToPage("/Books/Index");
        }

        var cart = HttpContext.Session.GetCart();

        var existing = cart.FirstOrDefault(x => x.BookId == book.Id && x.ItemType == normalizedType);
        if (existing is null)
        {
            existing = new CartItem
            {
                BookId = book.Id,
                Title = book.Title,
                Author = book.Author,
                CoverUrl = book.CoverUrl,
                ItemType = normalizedType,
                UnitPrice = normalizedType == "PhysicalBook" ? book.PricePrint : book.PriceEbook,
                Quantity = 0
            };

            cart.Add(existing);
        }

        existing.UnitPrice = normalizedType == "PhysicalBook" ? book.PricePrint : book.PriceEbook;

        if (normalizedType == "PhysicalBook")
        {
            existing.Quantity = Math.Min(existing.Quantity + quantity, Math.Max(book.Stock, 1));
        }
        else
        {
            existing.Quantity = Math.Min(existing.Quantity + quantity, 20);
        }

        HttpContext.Session.SaveCart(cart);

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        return RedirectToPage("/Cart/Index");
    }
}

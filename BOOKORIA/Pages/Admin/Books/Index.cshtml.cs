using BOOKORIA.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BOOKORIA.Pages.Admin.Books;

public class IndexModel(BookoriaDbContext dbContext) : PageModel
{
    public IReadOnlyList<BookRow> Books { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Books = await dbContext.Books
            .AsNoTracking()
            .OrderByDescending(x => x.IsActive)
            .ThenBy(x => x.Title)
            .Select(x => new BookRow(
                x.Id,
                x.Title,
                x.Author,
                x.PriceEbook,
                x.PricePrint,
                x.Stock,
                x.IsActive))
            .ToListAsync(cancellationToken);
    }

    public sealed record BookRow(
        Guid Id,
        string Title,
        string Author,
        decimal PriceEbook,
        decimal PricePrint,
        int Stock,
        bool IsActive);
}

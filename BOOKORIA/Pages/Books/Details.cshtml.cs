using BOOKORIA.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BOOKORIA.Pages.Books;

public class DetailsModel(BookoriaDbContext dbContext) : PageModel
{
    public BookDetailsViewModel? Book { get; private set; }

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        Book = await dbContext.Books
            .AsNoTracking()
            .Where(x => x.Id == id && x.IsActive)
            .Select(x => new BookDetailsViewModel(
                x.Id,
                x.Title,
                x.Author,
                x.Isbn,
                x.PriceEbook,
                x.PricePrint,
                x.Stock,
                x.Description,
                x.CoverUrl,
                x.FullPdfUrl,
                x.SamplePdfUrl))
            .FirstOrDefaultAsync(cancellationToken);

        if (Book is null)
        {
            return NotFound();
        }

        return Page();
    }

    public sealed record BookDetailsViewModel(
        Guid Id,
        string Title,
        string Author,
        string? Isbn,
        decimal PriceEbook,
        decimal PricePrint,
        int Stock,
        string Description,
        string? CoverUrl,
        string? FullPdfUrl,
        string? SamplePdfUrl);
}

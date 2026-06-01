using BOOKORIA.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BOOKORIA.Pages.Books;

public class IndexModel(BookoriaDbContext dbContext) : PageModel
{
    private static readonly int[] AllowedPageSizes = [9, 12, 24];

    [BindProperty(SupportsGet = true)]
    public string? Keyword { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid? CategoryId { get; set; }

    [BindProperty(SupportsGet = true)]
    public decimal? MinEbookPrice { get; set; }

    [BindProperty(SupportsGet = true)]
    public decimal? MaxEbookPrice { get; set; }

    [BindProperty(SupportsGet = true)]
    public decimal? MinPrintPrice { get; set; }

    [BindProperty(SupportsGet = true)]
    public decimal? MaxPrintPrice { get; set; }

    [BindProperty(SupportsGet = true)]
    public bool InStockOnly { get; set; }

    [BindProperty(SupportsGet = true)]
    public string SortBy { get; set; } = "title_asc";

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public int PageSize { get; set; } = 9;

    public IReadOnlyList<BookListItem> Books { get; private set; } = [];
    public IReadOnlyList<CategoryOption> Categories { get; private set; } = [];
    public int TotalItems { get; private set; }
    public int TotalPages { get; private set; }
    public int StartItem => TotalItems == 0 ? 0 : ((PageNumber - 1) * PageSize) + 1;
    public int EndItem => Math.Min(PageNumber * PageSize, TotalItems);

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        if (!AllowedPageSizes.Contains(PageSize))
        {
            PageSize = 9;
        }

        if (PageNumber < 1)
        {
            PageNumber = 1;
        }

        Categories = await dbContext.Categories
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new CategoryOption(x.Id, x.Name))
            .ToListAsync(cancellationToken);

        var query = dbContext.Books
            .AsNoTracking()
            .Where(x => x.IsActive)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(Keyword))
        {
            var keyword = Keyword.Trim();
            query = query.Where(x =>
                x.Title.Contains(keyword) ||
                x.Author.Contains(keyword) ||
                (x.Isbn != null && x.Isbn.Contains(keyword)));
        }

        if (CategoryId.HasValue)
        {
            query = query.Where(x => x.BookCategories.Any(bc => bc.CategoryId == CategoryId.Value));
        }

        if (MinEbookPrice.HasValue)
        {
            query = query.Where(x => x.PriceEbook >= MinEbookPrice.Value);
        }

        if (MaxEbookPrice.HasValue)
        {
            query = query.Where(x => x.PriceEbook <= MaxEbookPrice.Value);
        }

        if (MinPrintPrice.HasValue)
        {
            query = query.Where(x => x.PricePrint >= MinPrintPrice.Value);
        }

        if (MaxPrintPrice.HasValue)
        {
            query = query.Where(x => x.PricePrint <= MaxPrintPrice.Value);
        }

        if (InStockOnly)
        {
            query = query.Where(x => x.Stock > 0);
        }

        query = SortBy switch
        {
            "title_desc" => query.OrderByDescending(x => x.Title),
            "ebook_asc" => query.OrderBy(x => x.PriceEbook),
            "ebook_desc" => query.OrderByDescending(x => x.PriceEbook),
            "print_asc" => query.OrderBy(x => x.PricePrint),
            "print_desc" => query.OrderByDescending(x => x.PricePrint),
            _ => query.OrderBy(x => x.Title)
        };

        TotalItems = await query.CountAsync(cancellationToken);
        TotalPages = TotalItems == 0 ? 0 : (int)Math.Ceiling(TotalItems / (double)PageSize);

        if (TotalPages > 0 && PageNumber > TotalPages)
        {
            PageNumber = TotalPages;
        }

        Books = await query
            .Skip((PageNumber - 1) * PageSize)
            .Take(PageSize)
            .Select(x => new BookListItem(
                x.Id,
                x.Title,
                x.Author,
                x.PriceEbook,
                x.PricePrint,
                x.Stock,
                x.CoverUrl,
                x.Description))
            .ToListAsync(cancellationToken);
    }

    public sealed record BookListItem(
        Guid Id,
        string Title,
        string Author,
        decimal PriceEbook,
        decimal PricePrint,
        int Stock,
        string? CoverUrl,
        string Description);

    public sealed record CategoryOption(Guid Id, string Name);
}

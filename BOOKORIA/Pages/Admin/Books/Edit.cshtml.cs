using System.ComponentModel.DataAnnotations;
using BOOKORIA.Application.Abstractions;
using BOOKORIA.Domain.Entities;
using BOOKORIA.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BOOKORIA.Pages.Admin.Books;

public class EditModel(
    BookoriaDbContext dbContext,
    ICloudinaryStorageService cloudinaryStorageService) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public IReadOnlyList<CategoryItem> Categories { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        await LoadCategoriesAsync(cancellationToken);

        var book = await dbContext.Books
            .AsNoTracking()
            .Include(x => x.BookCategories)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (book is null)
        {
            return NotFound();
        }

        Input = new InputModel
        {
            Id = book.Id,
            Title = book.Title,
            Author = book.Author,
            Isbn = book.Isbn,
            PriceEbook = book.PriceEbook,
            PricePrint = book.PricePrint,
            Stock = book.Stock,
            Description = book.Description,
            IsActive = book.IsActive,
            SelectedCategoryIds = book.BookCategories.Select(x => x.CategoryId).ToList(),
            CurrentCoverUrl = book.CoverUrl,
            CurrentFullPdfUrl = book.FullPdfUrl,
            CurrentSamplePdfUrl = book.SamplePdfUrl
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        await LoadCategoriesAsync(cancellationToken);

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var book = await dbContext.Books
            .Include(x => x.BookCategories)
            .FirstOrDefaultAsync(x => x.Id == Input.Id, cancellationToken);

        if (book is null)
        {
            return NotFound();
        }

        book.Title = Input.Title.Trim();
        book.Author = Input.Author.Trim();
        book.Isbn = string.IsNullOrWhiteSpace(Input.Isbn) ? null : Input.Isbn.Trim();
        book.PriceEbook = Input.PriceEbook;
        book.PricePrint = Input.PricePrint;
        book.Stock = Input.Stock;
        book.Description = Input.Description.Trim();
        book.IsActive = Input.IsActive;

        if (Input.CoverFile is not null && Input.CoverFile.Length > 0)
        {
            await using var stream = Input.CoverFile.OpenReadStream();
            var cover = await cloudinaryStorageService.UploadBookCoverAsync(stream, Input.CoverFile.FileName, cancellationToken);
            book.CoverUrl = cover.Url;
        }

        if (Input.FullPdfFile is not null && Input.FullPdfFile.Length > 0)
        {
            await using var stream = Input.FullPdfFile.OpenReadStream();
            var fullPdf = await cloudinaryStorageService.UploadBookPdfAsync(stream, Input.FullPdfFile.FileName, false, cancellationToken);
            book.FullPdfUrl = fullPdf.Url;
        }

        if (Input.SamplePdfFile is not null && Input.SamplePdfFile.Length > 0)
        {
            await using var stream = Input.SamplePdfFile.OpenReadStream();
            var samplePdf = await cloudinaryStorageService.UploadBookPdfAsync(stream, Input.SamplePdfFile.FileName, true, cancellationToken);
            book.SamplePdfUrl = samplePdf.Url;
        }

        var selectedCategoryIds = Input.SelectedCategoryIds
            .Distinct()
            .ToHashSet();

        var validCategoryIds = await dbContext.Categories
            .Where(x => selectedCategoryIds.Contains(x.Id))
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        selectedCategoryIds = validCategoryIds.ToHashSet();

        var existingCategoryIds = book.BookCategories
            .Select(x => x.CategoryId)
            .ToHashSet();

        var toRemove = book.BookCategories
            .Where(x => !selectedCategoryIds.Contains(x.CategoryId))
            .ToList();

        foreach (var item in toRemove)
        {
            book.BookCategories.Remove(item);
        }

        foreach (var categoryId in selectedCategoryIds.Where(x => !existingCategoryIds.Contains(x)))
        {
            book.BookCategories.Add(new BookCategory
            {
                BookId = book.Id,
                CategoryId = categoryId
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return RedirectToPage("/Admin/Books/Index");
    }

    public sealed class InputModel
    {
        public Guid Id { get; set; }

        [Required]
        [MaxLength(300)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Author { get; set; } = string.Empty;

        [MaxLength(30)]
        public string? Isbn { get; set; }

        [Range(0, 999999999)]
        public decimal PriceEbook { get; set; }

        [Range(0, 999999999)]
        public decimal PricePrint { get; set; }

        [Range(0, int.MaxValue)]
        public int Stock { get; set; }

        [Required]
        [MaxLength(3000)]
        public string Description { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public List<Guid> SelectedCategoryIds { get; set; } = [];

        public string? CurrentCoverUrl { get; set; }
        public string? CurrentFullPdfUrl { get; set; }
        public string? CurrentSamplePdfUrl { get; set; }

        public IFormFile? CoverFile { get; set; }
        public IFormFile? FullPdfFile { get; set; }
        public IFormFile? SamplePdfFile { get; set; }
    }

    public sealed record CategoryItem(Guid Id, string Name);

    private async Task LoadCategoriesAsync(CancellationToken cancellationToken)
    {
        Categories = await dbContext.Categories
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new CategoryItem(x.Id, x.Name))
            .ToListAsync(cancellationToken);
    }
}

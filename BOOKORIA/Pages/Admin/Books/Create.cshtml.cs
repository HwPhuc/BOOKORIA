using System.ComponentModel.DataAnnotations;
using BOOKORIA.Application.Abstractions;
using BOOKORIA.Domain.Entities;
using BOOKORIA.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BOOKORIA.Pages.Admin.Books;

public class CreateModel(
    BookoriaDbContext dbContext,
    ICloudinaryStorageService cloudinaryStorageService) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var book = new Book
        {
            Id = Guid.NewGuid(),
            Title = Input.Title.Trim(),
            Author = Input.Author.Trim(),
            Isbn = string.IsNullOrWhiteSpace(Input.Isbn) ? null : Input.Isbn.Trim(),
            PriceEbook = Input.PriceEbook,
            PricePrint = Input.PricePrint,
            Stock = Input.Stock,
            Description = Input.Description.Trim(),
            IsActive = Input.IsActive
        };

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

        dbContext.Books.Add(book);
        await dbContext.SaveChangesAsync(cancellationToken);

        return RedirectToPage("/Admin/Books/Index");
    }

    public sealed class InputModel
    {
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

        public IFormFile? CoverFile { get; set; }
        public IFormFile? FullPdfFile { get; set; }
        public IFormFile? SamplePdfFile { get; set; }
    }
}

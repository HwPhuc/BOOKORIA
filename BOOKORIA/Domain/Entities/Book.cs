using System.ComponentModel.DataAnnotations;

namespace BOOKORIA.Domain.Entities;

public class Book
{
    public Guid Id { get; set; }

    [MaxLength(300)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(200)]
    public string Author { get; set; } = string.Empty;

    [MaxLength(30)]
    public string? Isbn { get; set; }

    public decimal PriceEbook { get; set; }
    public decimal PricePrint { get; set; }
    public int Stock { get; set; }

    [MaxLength(3000)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? CoverUrl { get; set; }

    [MaxLength(1000)]
    public string? FullPdfUrl { get; set; }

    [MaxLength(1000)]
    public string? SamplePdfUrl { get; set; }

    public bool IsActive { get; set; } = true;

    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public ICollection<BookCategory> BookCategories { get; set; } = new List<BookCategory>();
}

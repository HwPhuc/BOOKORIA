using System.ComponentModel.DataAnnotations;

namespace BOOKORIA.Domain.Entities;

public class Category
{
    public Guid Id { get; set; }

    [MaxLength(120)]
    public string Name { get; set; } = string.Empty;

    public ICollection<BookCategory> BookCategories { get; set; } = new List<BookCategory>();
}

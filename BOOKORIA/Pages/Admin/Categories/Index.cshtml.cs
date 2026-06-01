using BOOKORIA.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BOOKORIA.Pages.Admin.Categories;

public class IndexModel(BookoriaDbContext dbContext) : PageModel
{
    public IReadOnlyList<CategoryRow> Categories { get; private set; } = [];

    [TempData]
    public string? Message { get; set; }

    [TempData]
    public string? Error { get; set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Categories = await dbContext.Categories
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new CategoryRow(
                x.Id,
                x.Name,
                x.BookCategories.Count))
            .ToListAsync(cancellationToken);
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var category = await dbContext.Categories
            .Include(x => x.BookCategories)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (category is null)
        {
            Error = "Không tìm thấy thể loại.";
            return RedirectToPage();
        }

        if (category.BookCategories.Any())
        {
            Error = "Không thể xóa thể loại đang có sách liên kết.";
            return RedirectToPage();
        }

        dbContext.Categories.Remove(category);
        await dbContext.SaveChangesAsync(cancellationToken);

        Message = "Đã xóa thể loại thành công.";
        return RedirectToPage();
    }

    public sealed record CategoryRow(Guid Id, string Name, int BookCount);
}

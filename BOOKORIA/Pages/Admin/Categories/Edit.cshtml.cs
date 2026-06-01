using System.ComponentModel.DataAnnotations;
using BOOKORIA.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BOOKORIA.Pages.Admin.Categories;

public class EditModel(BookoriaDbContext dbContext) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        var category = await dbContext.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (category is null)
        {
            return NotFound();
        }

        Input = new InputModel
        {
            Id = category.Id,
            Name = category.Name
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var category = await dbContext.Categories
            .FirstOrDefaultAsync(x => x.Id == Input.Id, cancellationToken);

        if (category is null)
        {
            return NotFound();
        }

        var normalizedName = Input.Name.Trim();
        var exists = await dbContext.Categories
            .AnyAsync(x => x.Id != Input.Id && x.Name.ToLower() == normalizedName.ToLower(), cancellationToken);

        if (exists)
        {
            ModelState.AddModelError(nameof(Input.Name), "Tên thể loại đã tồn tại.");
            return Page();
        }

        category.Name = normalizedName;
        await dbContext.SaveChangesAsync(cancellationToken);

        return RedirectToPage("/Admin/Categories/Index");
    }

    public sealed class InputModel
    {
        public Guid Id { get; set; }

        [Required]
        [MaxLength(120)]
        [Display(Name = "Tên thể loại")]
        public string Name { get; set; } = string.Empty;
    }
}

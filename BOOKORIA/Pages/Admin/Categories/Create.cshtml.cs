using System.ComponentModel.DataAnnotations;
using BOOKORIA.Domain.Entities;
using BOOKORIA.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BOOKORIA.Pages.Admin.Categories;

public class CreateModel(BookoriaDbContext dbContext) : PageModel
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

        var normalizedName = Input.Name.Trim();
        var exists = await dbContext.Categories
            .AnyAsync(x => x.Name.ToLower() == normalizedName.ToLower(), cancellationToken);

        if (exists)
        {
            ModelState.AddModelError(nameof(Input.Name), "Tên thể loại đã tồn tại.");
            return Page();
        }

        dbContext.Categories.Add(new Category
        {
            Id = Guid.NewGuid(),
            Name = normalizedName
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        return RedirectToPage("/Admin/Categories/Index");
    }

    public sealed class InputModel
    {
        [Required]
        [MaxLength(120)]
        [Display(Name = "Tên thể loại")]
        public string Name { get; set; } = string.Empty;
    }
}

using BOOKORIA.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BOOKORIA.Pages.Books;

public class ReadSampleModel(BookoriaDbContext dbContext) : PageModel
{
    public SampleViewModel? Sample { get; private set; }

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        Sample = await dbContext.Books
            .AsNoTracking()
            .Where(x => x.Id == id && x.IsActive && x.SamplePdfUrl != null)
            .Select(x => new SampleViewModel(x.Id, x.Title, x.SamplePdfUrl!))
            .FirstOrDefaultAsync(cancellationToken);

        if (Sample is null)
        {
            return NotFound();
        }

        return Page();
    }

    public sealed record SampleViewModel(Guid BookId, string Title, string SamplePdfUrl);
}

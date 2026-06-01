using BOOKORIA.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BOOKORIA.Pages
{
    public class IndexModel(BookoriaDbContext dbContext) : PageModel
    {
        public IReadOnlyList<HomeBookItem> FeaturedBooks { get; private set; } = [];
        public IReadOnlyList<HomeBookItem> NewBooks { get; private set; } = [];
        public int TotalBooks { get; private set; }
        public int TotalCustomers { get; private set; }
        public int SuccessfulOrders { get; private set; }

        public async Task OnGetAsync(CancellationToken cancellationToken)
        {
            TotalBooks = await dbContext.Books
                .AsNoTracking()
                .CountAsync(x => x.IsActive, cancellationToken);

            TotalCustomers = await dbContext.UserRoles
                .AsNoTracking()
                .Join(dbContext.Roles,
                    userRole => userRole.RoleId,
                    role => role.Id,
                    (userRole, role) => new { userRole.UserId, role.Name })
                .CountAsync(x => x.Name == "Customer", cancellationToken);

            SuccessfulOrders = await dbContext.Orders
                .AsNoTracking()
                .CountAsync(x => x.PaymentStatus == Domain.Enums.PaymentStatus.Succeeded, cancellationToken);

            FeaturedBooks = await dbContext.Books
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderBy(x => x.PriceEbook)
                .Take(4)
                .Select(x => new HomeBookItem(x.Id, x.Title, x.Author, x.CoverUrl, x.PriceEbook, x.PricePrint))
                .ToListAsync(cancellationToken);

            NewBooks = await dbContext.Books
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderByDescending(x => x.Id)
                .Take(4)
                .Select(x => new HomeBookItem(x.Id, x.Title, x.Author, x.CoverUrl, x.PriceEbook, x.PricePrint))
                .ToListAsync(cancellationToken);
        }

        public sealed record HomeBookItem(
            Guid Id,
            string Title,
            string Author,
            string? CoverUrl,
            decimal PriceEbook,
            decimal PricePrint);
    }
}

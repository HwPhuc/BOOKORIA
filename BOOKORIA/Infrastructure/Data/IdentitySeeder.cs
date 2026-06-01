using BOOKORIA.Infrastructure.Identity;
using BOOKORIA.Infrastructure.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace BOOKORIA.Infrastructure.Data;

public static class IdentitySeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var options = scope.ServiceProvider.GetRequiredService<IOptions<IdentitySeedOptions>>().Value;

        await EnsureRoleAsync(roleManager, "Admin");
        await EnsureRoleAsync(roleManager, "Customer");

        await EnsureUserAsync(userManager, options.Admin, "Admin", cancellationToken);
        await EnsureUserAsync(userManager, options.Customer, "Customer", cancellationToken);
    }

    private static async Task EnsureRoleAsync(RoleManager<IdentityRole> roleManager, string role)
    {
        if (await roleManager.RoleExistsAsync(role))
        {
            return;
        }

        await roleManager.CreateAsync(new IdentityRole(role));
    }

    private static async Task EnsureUserAsync(
        UserManager<ApplicationUser> userManager,
        SeedUser seedUser,
        string role,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(seedUser.Username) || string.IsNullOrWhiteSpace(seedUser.Password))
        {
            return;
        }

        var user = await userManager.FindByNameAsync(seedUser.Username);
        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = seedUser.Username,
                Email = string.IsNullOrWhiteSpace(seedUser.Email) ? null : seedUser.Email,
                EmailConfirmed = true
            };

            var createResult = await userManager.CreateAsync(user, seedUser.Password);
            if (!createResult.Succeeded)
            {
                var errors = string.Join("; ", createResult.Errors.Select(x => x.Description));
                throw new InvalidOperationException($"Create seed user '{seedUser.Username}' failed: {errors}");
            }
        }

        if (!await userManager.IsInRoleAsync(user, role))
        {
            await userManager.AddToRoleAsync(user, role);
        }
    }
}

using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using BOOKORIA.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BOOKORIA.Pages.Account;

public class LoginModel(
    SignInManager<ApplicationUser> signInManager,
    UserManager<ApplicationUser> userManager) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public IActionResult OnGet()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToRoleHome(User.FindFirstValue(ClaimTypes.Role));
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await signInManager.PasswordSignInAsync(
            Input.Username,
            Input.Password,
            Input.RememberMe,
            lockoutOnFailure: false);

        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, "Sai tài khoản hoặc mật khẩu.");
            return Page();
        }

        var user = await userManager.FindByNameAsync(Input.Username);
        if (user is null)
        {
            return RedirectToPage("/Index");
        }

        var roles = await userManager.GetRolesAsync(user);
        return RedirectToRoleHome(roles.FirstOrDefault());
    }

    private IActionResult RedirectToRoleHome(string? role)
    {
        return role switch
        {
            "Admin" => RedirectToPage("/Admin/Books/Index"),
            "Customer" => RedirectToPage("/Books/Index"),
            _ => RedirectToPage("/Index")
        };
    }

    public sealed class InputModel
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
    }
}

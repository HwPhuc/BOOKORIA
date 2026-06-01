using System.ComponentModel.DataAnnotations;
using BOOKORIA.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BOOKORIA.Pages.Account;

[Authorize]
public class ProfileModel(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    [TempData]
    public string? Message { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Challenge();
        }

        Input = new InputModel
        {
            Username = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Challenge();
        }

        var newUsername = Input.Username.Trim();
        var newEmail = Input.Email.Trim();
        var newPhoneNumber = string.IsNullOrWhiteSpace(Input.PhoneNumber)
            ? null
            : Input.PhoneNumber.Trim();

        var existingByUsername = await userManager.FindByNameAsync(newUsername);
        if (existingByUsername is not null && existingByUsername.Id != user.Id)
        {
            ModelState.AddModelError(nameof(Input.Username), "Tên đăng nhập đã tồn tại.");
            return Page();
        }

        var existingByEmail = await userManager.FindByEmailAsync(newEmail);
        if (existingByEmail is not null && existingByEmail.Id != user.Id)
        {
            ModelState.AddModelError(nameof(Input.Email), "Email đã được sử dụng.");
            return Page();
        }

        user.UserName = newUsername;
        user.Email = newEmail;
        user.PhoneNumber = newPhoneNumber;

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return Page();
        }

        await signInManager.RefreshSignInAsync(user);
        Message = "Cập nhật thông tin cá nhân thành công.";

        return RedirectToPage();
    }

    public sealed class InputModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập.")]
        [MaxLength(100)]
        [Display(Name = "Tên đăng nhập")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập email.")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
        [Display(Name = "Số điện thoại")]
        public string? PhoneNumber { get; set; }
    }
}

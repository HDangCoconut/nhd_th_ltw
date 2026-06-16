#nullable disable

using System.ComponentModel.DataAnnotations;
using System.Threading;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using NguyenHaiDang_W345.Models;

namespace NguyenHaiDang_W345.Areas.Identity.Pages.Account;

public class RegisterModel(
    UserManager<ApplicationUser> userManager,
    IUserStore<ApplicationUser> userStore,
    SignInManager<ApplicationUser> signInManager,
    RoleManager<IdentityRole> roleManager,
    ILogger<RegisterModel> logger) : PageModel
{
    private static readonly string[] SupportedRoles =
        { SD.Role_Customer, SD.Role_Company, SD.Role_Admin, SD.Role_Employee };

    [BindProperty]
    public InputModel Input { get; set; }
    public string ReturnUrl { get; set; }
    public IEnumerable<SelectListItem> RoleList { get; set; }
    public bool CanAssignRoles => User.IsInRole(SD.Role_Admin);

    public class InputModel
    {
        [Required(ErrorMessage = "Họ tên không được để trống"), StringLength(100, ErrorMessage = "Họ tên không được vượt quá 100 ký tự"), Display(Name = "Họ tên đầy đủ")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Tuổi không được để trống"), Range(1, 120, ErrorMessage = "Tuổi phải nằm trong khoảng từ 1 đến 120"), Display(Name = "Tuổi")]
        public int Age { get; set; }

        [StringLength(200, ErrorMessage = "Địa chỉ không được vượt quá 200 ký tự"), Display(Name = "Địa chỉ")]
        public string Address { get; set; }

        [Display(Name = "Quyền")]
        public string Role { get; set; }

        [Required, EmailAddress, Display(Name = "Email")]
        public string Email { get; set; }

        [Required, StringLength(100, ErrorMessage = "Mật khẩu phải có ít nhất {2} và tối đa {1} ký tự.", MinimumLength = 6), DataType(DataType.Password), Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password), Display(Name = "Confirm password"), Compare(nameof(Password), ErrorMessage = "Mật khẩu xác nhận không khớp.")]
        public string ConfirmPassword { get; set; }
    }

    public async Task OnGetAsync(string returnUrl = null)
    {
        ReturnUrl = returnUrl;
        await LoadRoleListAsync();
    }

    public async Task<IActionResult> OnPostAsync(string returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");
        await LoadRoleListAsync();

        if (!ModelState.IsValid) return Page();

        var user = CreateUserFromInput();
        await userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
        await GetEmailStore().SetEmailAsync(user, Input.Email, CancellationToken.None);

        var result = await userManager.CreateAsync(user, Input.Password);
        if (!result.Succeeded)
        {
            AddIdentityErrors(result);
            return Page();
        }

        await userManager.AddToRoleAsync(user, ResolveRole(Input.Role));
        await signInManager.SignInAsync(user, isPersistent: false);
        logger.LogInformation("User created a new account with password.");
        return LocalRedirect(returnUrl);
    }

    private ApplicationUser CreateUserFromInput() =>
        new() { FullName = Input.FullName, Age = Input.Age, Address = Input.Address };

    private string ResolveRole(string role) =>
        CanAssignRoles && SupportedRoles.Contains(role) ? role : SD.Role_Customer;

    private async Task LoadRoleListAsync()
    {
        foreach (var role in SupportedRoles)
            if (!await roleManager.RoleExistsAsync(role)) await roleManager.CreateAsync(new IdentityRole(role));

        RoleList = roleManager.Roles
            .OrderBy(role => role.Name)
            .Select(role => new SelectListItem { Text = role.Name, Value = role.Name })
            .ToList();
    }

    private IUserEmailStore<ApplicationUser> GetEmailStore()
    {
        if (!userManager.SupportsUserEmail)
            throw new NotSupportedException("The default UI requires a user store with email support.");

        return (IUserEmailStore<ApplicationUser>)userStore;
    }

    private void AddIdentityErrors(IdentityResult result)
    {
        foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description);
    }
}

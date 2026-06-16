using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NguyenHaiDang_W345.Areas.Admin.Models;
using NguyenHaiDang_W345.Models;

namespace NguyenHaiDang_W345.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = SD.Role_Admin)]
public class UserController(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager) : Controller
{
    private const string MissingRoleText = "Chưa có quyền";

    private static readonly string[] SupportedRoles =
        { SD.Role_Customer, SD.Role_Company, SD.Role_Admin, SD.Role_Employee };

    public async Task<IActionResult> Index()
    {
        await EnsureRolesAsync();

        var users = await userManager.Users
            .OrderBy(user => user.Email)
            .ToListAsync();

        return View(await Task.WhenAll(users.Select(BuildListItemAsync)));
    }

    public async Task<IActionResult> Details(string id) => await DetailsViewAsync(id);

    public async Task<IActionResult> Create()
    {
        await EnsureRolesAsync();
        return View(FormWithRoles(new AdminUserFormViewModel { Role = SD.Role_Customer }));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AdminUserFormViewModel model)
    {
        await EnsureRolesAsync();
        FormWithRoles(model);
        ValidateCreateForm(model);

        if (!ModelState.IsValid) return View(model);

        var user = CreateUserFromModel(model);
        var result = await userManager.CreateAsync(user, model.Password!);

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, model.Role);
            return RedirectToAction(nameof(Index));
        }

        AddIdentityErrors(result);
        return View(model);
    }

    public async Task<IActionResult> Edit(string id)
    {
        await EnsureRolesAsync();

        var user = await userManager.FindByIdAsync(id);

        if (user is null) return NotFound();

        return View(await BuildFormAsync(user));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, AdminUserFormViewModel model)
    {
        await EnsureRolesAsync();
        FormWithRoles(model);

        if (id != model.Id) return NotFound();
        ValidateEditForm(id, model);
        if (!ModelState.IsValid) return View(model);

        var user = await userManager.FindByIdAsync(id);

        if (user is null) return NotFound();

        if (!await TryUpdateProfileAsync(user, model)
            || !await TryUpdateRoleAsync(user, model.Role)
            || !await TryResetPasswordAsync(user, model.Password))
        {
            return View(model);
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(string id) => await DetailsViewAsync(id);

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string id)
    {
        var user = await userManager.FindByIdAsync(id);

        if (user is null) return NotFound();

        if (IsCurrentUser(user.Id)) return ErrorRedirect("Bạn không thể xóa chính tài khoản đang đăng nhập.");

        var result = await userManager.DeleteAsync(user);

        if (!result.Succeeded)
        {
            AddIdentityErrors(result);
            return View("Delete", await BuildDetailsAsync(user));
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleLock(string id)
    {
        var user = await userManager.FindByIdAsync(id);

        if (user is null) return NotFound();

        if (IsCurrentUser(user.Id)) return ErrorRedirect("Bạn không thể khóa chính tài khoản đang đăng nhập.");

        await userManager.SetLockoutEnabledAsync(user, true);
        await userManager.SetLockoutEndDateAsync(
            user,
            IsLocked(user) ? (DateTimeOffset?)null : DateTimeOffset.UtcNow.AddYears(100));
        return RedirectToAction(nameof(Index));
    }

    private async Task<AdminUserListItemViewModel> BuildListItemAsync(ApplicationUser user) =>
        new() { Id = user.Id, Email = user.Email ?? string.Empty, FullName = user.FullName, Age = user.Age, Roles = await GetRoleNamesAsync(user), IsLocked = IsLocked(user) };

    private async Task<IActionResult> DetailsViewAsync(string id)
    {
        var user = await userManager.FindByIdAsync(id);
        return user is null ? NotFound() : View(await BuildDetailsAsync(user));
    }

    private async Task<AdminUserDetailsViewModel> BuildDetailsAsync(ApplicationUser user) =>
        new() { Id = user.Id, Email = user.Email ?? string.Empty, FullName = user.FullName, Age = user.Age, Address = user.Address, PhoneNumber = user.PhoneNumber, Roles = await GetRoleNamesAsync(user), IsLocked = IsLocked(user) };

    private async Task<AdminUserFormViewModel> BuildFormAsync(ApplicationUser user)
    {
        var role = (await userManager.GetRolesAsync(user)).FirstOrDefault() ?? SD.Role_Customer;

        return FormWithRoles(new AdminUserFormViewModel { Id = user.Id, Email = user.Email ?? string.Empty, FullName = user.FullName, Age = user.Age, Address = user.Address, PhoneNumber = user.PhoneNumber, Role = role });
    }

    private AdminUserFormViewModel FormWithRoles(AdminUserFormViewModel model)
    {
        model.RoleList = GetRoleList(model.Role);
        return model;
    }

    private static ApplicationUser CreateUserFromModel(AdminUserFormViewModel model) =>
        new() { UserName = model.Email, Email = model.Email, FullName = model.FullName, Age = model.Age, Address = model.Address, PhoneNumber = model.PhoneNumber, LockoutEnabled = true };

    private void ValidateCreateForm(AdminUserFormViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Password))
            ModelState.AddModelError(nameof(model.Password), "Mật khẩu không được để trống.");
        ValidateRole(model.Role);
    }

    private void ValidateEditForm(string id, AdminUserFormViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Password) && string.IsNullOrWhiteSpace(model.ConfirmPassword))
        {
            ModelState.Remove(nameof(model.Password));
            ModelState.Remove(nameof(model.ConfirmPassword));
        }

        ValidateRole(model.Role);
        if (IsCurrentUser(id) && model.Role != SD.Role_Admin)
            AddModelError(nameof(model.Role), "Bạn không thể hạ quyền Admin của chính tài khoản đang đăng nhập.");
    }

    private void ValidateRole(string role)
    {
        if (!IsSupportedRole(role)) AddModelError(nameof(AdminUserFormViewModel.Role), "Quyền không hợp lệ.");
    }

    private async Task<bool> TryUpdateProfileAsync(ApplicationUser user, AdminUserFormViewModel model)
    {
        user.FullName = model.FullName;
        user.Age = model.Age;
        user.Address = model.Address;
        user.PhoneNumber = model.PhoneNumber;

        return await TryIdentityAsync(userManager.SetEmailAsync(user, model.Email))
            && await TryIdentityAsync(userManager.SetUserNameAsync(user, model.Email))
            && await TryIdentityAsync(userManager.UpdateAsync(user));
    }

    private async Task<bool> TryUpdateRoleAsync(ApplicationUser user, string role)
    {
        var currentRoles = await userManager.GetRolesAsync(user);

        return await TryIdentityAsync(userManager.RemoveFromRolesAsync(user, currentRoles))
            && await TryIdentityAsync(userManager.AddToRoleAsync(user, role));
    }

    private async Task<bool> TryResetPasswordAsync(ApplicationUser user, string? password)
    {
        if (string.IsNullOrWhiteSpace(password)) return true;

        var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
        return await TryIdentityAsync(userManager.ResetPasswordAsync(user, resetToken, password));
    }

    private async Task<bool> TryIdentityAsync(Task<IdentityResult> identityTask)
    {
        var result = await identityTask;

        if (result.Succeeded) return true;

        AddIdentityErrors(result);
        return false;
    }

    private async Task EnsureRolesAsync()
    {
        foreach (var role in SupportedRoles)
        {
            if (!await roleManager.RoleExistsAsync(role)) await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    private IEnumerable<SelectListItem> GetRoleList(string? selectedRole) =>
        SupportedRoles.Select(role => new SelectListItem { Text = role, Value = role, Selected = role == selectedRole });

    private async Task<string> GetRoleNamesAsync(ApplicationUser user)
    {
        var roles = await userManager.GetRolesAsync(user);
        return roles.Any() ? string.Join(", ", roles) : MissingRoleText;
    }

    private static bool IsSupportedRole(string role) => SupportedRoles.Contains(role);

    private static bool IsLocked(ApplicationUser user) =>
        user.LockoutEnd is not null && user.LockoutEnd > DateTimeOffset.UtcNow;

    private bool IsCurrentUser(string userId) => userManager.GetUserId(User) == userId;

    private IActionResult ErrorRedirect(string message)
    {
        TempData["Error"] = message;
        return RedirectToAction(nameof(Index));
    }

    private void AddModelError(string key, string message) => ModelState.AddModelError(key, message);

    private void AddIdentityErrors(IdentityResult result)
    {
        foreach (var error in result.Errors) AddModelError(string.Empty, error.Description);
    }
}

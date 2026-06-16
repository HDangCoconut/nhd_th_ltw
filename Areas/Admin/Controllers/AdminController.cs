using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NguyenHaiDang_W345.Models;
using NguyenHaiDang_W345.Repositories;

namespace NguyenHaiDang_W345.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = SD.Role_Admin)]
public class AdminController(
    IProductRepository productRepository,
    ICategoryRepository categoryRepository,
    UserManager<ApplicationUser> userManager) : Controller
{
    public async Task<IActionResult> Index()
    {
        ViewBag.ProductCount = (await productRepository.GetAllAsync()).Count();
        ViewBag.CategoryCount = (await categoryRepository.GetAllAsync()).Count();
        ViewBag.UserCount = await userManager.Users.CountAsync();
        return View();
    }
}

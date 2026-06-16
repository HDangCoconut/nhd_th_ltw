using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NguyenHaiDang_W345.Models;

namespace NguyenHaiDang_W345.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = SD.Role_Admin)]
public class OrderController(ApplicationDbContext context) : Controller
{
    public async Task<IActionResult> Index()
    {
        var orders = await context.Orders
            .Include(order => order.ApplicationUser)
            .Include(order => order.OrderDetails)
            .ThenInclude(detail => detail.Product)
            .OrderByDescending(order => order.OrderDate)
            .ToListAsync();

        return View(orders);
    }
}

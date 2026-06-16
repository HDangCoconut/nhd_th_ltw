using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NguyenHaiDang_W345.Extensions;
using NguyenHaiDang_W345.Models;
using NguyenHaiDang_W345.Repositories;

namespace NguyenHaiDang_W345.Controllers;

public class ShoppingCartController(
    IProductRepository productRepository,
    ApplicationDbContext context,
    UserManager<ApplicationUser> userManager) : Controller
{
    public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
    {
        var product = await productRepository.GetByIdAsync(productId);
        if (product is null) return NotFound();

        var cart = GetCart();
        cart.AddItem(new CartItem { ProductId = productId, Name = product.Name, Price = product.Price, Quantity = quantity });
        SaveCart(cart);
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Index() => View(GetCart());

    public IActionResult RemoveFromCart(int productId)
    {
        var cart = GetCart();
        cart.RemoveItem(productId);
        SaveCart(cart);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetCartItem(int productId, int quantity)
    {
        if (quantity < 1) quantity = 1;
        var product = await productRepository.GetByIdAsync(productId);
        if (product is null) return NotFound();

        var cart = GetCart();
        cart.SetItem(new CartItem { ProductId = productId, Name = product.Name, Price = product.Price }, quantity);
        SaveCart(cart);
        return RedirectToAction("Display", "Product", new { id = productId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult UpdateQuantity(int productId, int delta)
    {
        var cart = GetCart();
        cart.ChangeQuantity(productId, delta);
        SaveCart(cart);
        return RedirectToAction(nameof(Index));
    }

    [Authorize]
    public async Task<IActionResult> History()
    {
        var userId = userManager.GetUserId(User);
        var orders = await context.Orders
            .Include(order => order.OrderDetails)
            .ThenInclude(detail => detail.Product)
            .Where(order => order.UserId == userId)
            .OrderByDescending(order => order.OrderDate)
            .ToListAsync();

        return View(orders);
    }

    [Authorize]
    public IActionResult Checkout() => GetCart().Items.Any() ? View(new Order()) : RedirectToAction(nameof(Index));

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout(Order order)
    {
        var cart = GetCart();
        if (!cart.Items.Any()) return RedirectToAction(nameof(Index));

        var user = await userManager.GetUserAsync(User);
        if (user is null) return Challenge();

        order.UserId = user.Id;
        order.OrderDate = DateTime.UtcNow;
        order.TotalPrice = cart.Items.Sum(item => item.Price * item.Quantity);
        order.OrderDetails = cart.Items.Select(item => new OrderDetail
        {
            ProductId = item.ProductId,
            Quantity = item.Quantity,
            Price = item.Price
        }).ToList();

        context.Orders.Add(order);
        await context.SaveChangesAsync();
        HttpContext.ClearShoppingCart();
        return View("OrderCompleted", order.Id);
    }

    private ShoppingCart GetCart() => HttpContext.GetShoppingCart();

    private void SaveCart(ShoppingCart cart) => HttpContext.SaveShoppingCart(cart);
}

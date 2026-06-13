using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NguyenHaiDang_W345.Extensions;
using NguyenHaiDang_W345.Models;
using NguyenHaiDang_W345.Repositories;

namespace NguyenHaiDang_W345.Controllers;

// Bài 5 - 5.2.1: Controller xử lý các thao tác giỏ hàng bằng Session.
public class ShoppingCartController : Controller
{
    private const string CartSessionKey = "Cart";

    private readonly IProductRepository _productRepository;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public ShoppingCartController(
        IProductRepository productRepository,
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _productRepository = productRepository;
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
    {
        var product = await GetProductFromDatabase(productId);

        if (product is null)
        {
            return NotFound();
        }

        var cartItem = new CartItem
        {
            ProductId = productId,
            Name = product.Name,
            Price = product.Price,
            Quantity = quantity
        };

        var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>(CartSessionKey) ?? new ShoppingCart();
        cart.AddItem(cartItem);
        HttpContext.Session.SetObjectAsJson(CartSessionKey, cart);

        return RedirectToAction(nameof(Index));
    }

    public IActionResult Index()
    {
        var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>(CartSessionKey) ?? new ShoppingCart();
        return View(cart);
    }

    public IActionResult RemoveFromCart(int productId)
    {
        var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>(CartSessionKey);

        if (cart is not null)
        {
            cart.RemoveItem(productId);
            HttpContext.Session.SetObjectAsJson(CartSessionKey, cart);
        }

        return RedirectToAction(nameof(Index));
    }

    // Bài 5 - 5.2.2: Hiển thị form nhập thông tin giao hàng trước khi đặt hàng.
    [Authorize]
    public IActionResult Checkout()
    {
        var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>(CartSessionKey) ?? new ShoppingCart();

        if (!cart.Items.Any())
        {
            return RedirectToAction(nameof(Index));
        }

        return View(new Order());
    }

    // Bài 5 - 5.2.2: Lưu đơn hàng và chi tiết đơn hàng từ giỏ hàng trong Session.
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout(Order order)
    {
        var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>(CartSessionKey);

        if (cart is null || !cart.Items.Any())
        {
            return RedirectToAction(nameof(Index));
        }

        var user = await _userManager.GetUserAsync(User);

        if (user is null)
        {
            return Challenge();
        }

        order.UserId = user.Id;
        order.OrderDate = DateTime.UtcNow;
        order.TotalPrice = cart.Items.Sum(item => item.Price * item.Quantity);
        order.OrderDetails = cart.Items.Select(item => new OrderDetail
        {
            ProductId = item.ProductId,
            Quantity = item.Quantity,
            Price = item.Price
        }).ToList();

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        HttpContext.Session.Remove(CartSessionKey);

        return View("OrderCompleted", order.Id);
    }

    private async Task<Product?> GetProductFromDatabase(int productId)
    {
        return await _productRepository.GetByIdAsync(productId);
    }
}

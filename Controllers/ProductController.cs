using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NguyenHaiDang_W345.Extensions;
using NguyenHaiDang_W345.Models;
using NguyenHaiDang_W345.Repositories;

namespace NguyenHaiDang_W345.Controllers;

public class ProductController(
    IProductRepository productRepository,
    ICategoryRepository categoryRepository,
    IWebHostEnvironment webHostEnvironment) : Controller
{
    private const int HomePageSize = 8;
    private const int AdminListPageSize = 10;
    private static readonly HashSet<string> AllowedImageExtensions =
        new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

    public async Task<IActionResult> Index(string? keyword, int page = 1)
    {
        var products = ApplyKeyword(await productRepository.GetAllAsync(), keyword);
        ViewBag.Categories = await categoryRepository.GetAllAsync();
        ViewBag.Keyword = keyword;
        return View(Paginate(products, page, HomePageSize));
    }

    [Authorize(Roles = SD.Role_Admin)]
    public async Task<IActionResult> List(int page = 1) =>
        View(Paginate(await productRepository.GetAllAsync(), page, AdminListPageSize));

    [Authorize(Roles = SD.Role_Admin)]
    public async Task<IActionResult> Add()
    {
        await LoadCategoriesAsync();
        return View();
    }

    [HttpPost]
    [Authorize(Roles = SD.Role_Admin)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(Product product, IFormFile? imageUrl)
    {
        ValidateImage(imageUrl);
        if (!ModelState.IsValid) return await ProductFormViewAsync(product);

        if (imageUrl is not null) product.ImageUrl = await SaveImageAsync(imageUrl);
        await productRepository.AddAsync(product);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Display(int id)
    {
        var product = await productRepository.GetByIdAsync(id);
        if (product is null) return NotFound();

        ViewBag.CartQuantity = HttpContext.GetShoppingCart().GetQuantity(id);
        return View(product);
    }

    [Authorize(Roles = SD.Role_Admin)]
    public async Task<IActionResult> Update(int id)
    {
        var product = await productRepository.GetByIdAsync(id);
        if (product is null) return NotFound();

        await LoadCategoriesAsync(product.CategoryId);
        return View(product);
    }

    [HttpPost]
    [Authorize(Roles = SD.Role_Admin)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(int id, Product product, IFormFile? imageUrl)
    {
        if (id != product.Id) return NotFound();

        ValidateImage(imageUrl);
        if (!ModelState.IsValid) return await ProductFormViewAsync(product);

        var existingProduct = await productRepository.GetByIdAsync(id);
        if (existingProduct is null) return NotFound();

        UpdateProduct(existingProduct, product);
        if (imageUrl is not null) existingProduct.ImageUrl = await SaveImageAsync(imageUrl);

        await productRepository.UpdateAsync(existingProduct);
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = SD.Role_Admin)]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await productRepository.GetByIdAsync(id);
        return product is null ? NotFound() : View(product);
    }

    [HttpPost]
    [Authorize(Roles = SD.Role_Admin)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await productRepository.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }

    private async Task<IActionResult> ProductFormViewAsync(Product product)
    {
        await LoadCategoriesAsync(product.CategoryId);
        return View(product);
    }

    private async Task LoadCategoriesAsync(int? selectedCategoryId = null) =>
        ViewBag.Categories = new SelectList(await categoryRepository.GetAllAsync(), "Id", "Name", selectedCategoryId);

    private static IEnumerable<Product> ApplyKeyword(IEnumerable<Product> products, string? keyword) =>
        string.IsNullOrWhiteSpace(keyword)
            ? products
            : products.Where(product => product.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                || (product.Description?.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false));

    private List<Product> Paginate(IEnumerable<Product> products, int page, int pageSize)
    {
        var list = products.ToList();
        var totalPages = (int)Math.Ceiling(list.Count / (double)pageSize);
        var currentPage = totalPages == 0 ? 1 : Math.Clamp(page, 1, totalPages);

        ViewBag.CurrentPage = currentPage;
        ViewBag.TotalPages = totalPages;
        ViewBag.TotalItems = list.Count;
        ViewBag.PageSize = pageSize;

        return list.Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();
    }

    private async Task<string> SaveImageAsync(IFormFile image)
    {
        var imagesDirectory = Path.Combine(webHostEnvironment.WebRootPath, "images");
        Directory.CreateDirectory(imagesDirectory);

        var fileName = $"{Guid.NewGuid():N}{Path.GetExtension(image.FileName)}";
        await using var fileStream = new FileStream(Path.Combine(imagesDirectory, fileName), FileMode.Create);
        await image.CopyToAsync(fileStream);
        return $"/images/{fileName}";
    }

    private void ValidateImage(IFormFile? image)
    {
        if (image is not null && !IsValidImage(image))
            ModelState.AddModelError(nameof(Product.ImageUrl), "Tệp tải lên phải là hình ảnh hợp lệ.");
    }

    private static bool IsValidImage(IFormFile image) =>
        image.Length > 0
        && image.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase)
        && AllowedImageExtensions.Contains(Path.GetExtension(image.FileName));

    private static void UpdateProduct(Product existingProduct, Product product)
    {
        existingProduct.Name = product.Name;
        existingProduct.Price = product.Price;
        existingProduct.Description = product.Description;
        existingProduct.CategoryId = product.CategoryId;
    }
}

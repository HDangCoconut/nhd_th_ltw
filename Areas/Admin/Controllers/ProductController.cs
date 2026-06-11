using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NguyenHaiDang_W345.Models;
using NguyenHaiDang_W345.Repositories;

namespace NguyenHaiDang_W345.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = SD.Role_Admin)]
public class ProductController : Controller
{
    private static readonly HashSet<string> AllowedImageExtensions =
        new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public ProductController(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        IWebHostEnvironment webHostEnvironment)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _webHostEnvironment = webHostEnvironment;
    }

    public async Task<IActionResult> Index()
    {
        var products = await _productRepository.GetAllAsync();
        return View(products);
    }

    public async Task<IActionResult> Display(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);

        if (product is null)
        {
            return NotFound();
        }

        return View(product);
    }

    public async Task<IActionResult> Add()
    {
        await LoadCategoriesAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(Product product, IFormFile? imageUrl)
    {
        if (imageUrl is not null && !IsValidImage(imageUrl))
        {
            ModelState.AddModelError(nameof(Product.ImageUrl), "Tệp tải lên phải là hình ảnh hợp lệ.");
        }

        if (ModelState.IsValid)
        {
            if (imageUrl is not null)
            {
                product.ImageUrl = await SaveImageAsync(imageUrl);
            }

            await _productRepository.AddAsync(product);
            return RedirectToAction(nameof(Index));
        }

        await LoadCategoriesAsync(product.CategoryId);
        return View(product);
    }

    public async Task<IActionResult> Update(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);

        if (product is null)
        {
            return NotFound();
        }

        await LoadCategoriesAsync(product.CategoryId);
        return View(product);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(int id, Product product, IFormFile? imageUrl)
    {
        if (id != product.Id)
        {
            return NotFound();
        }

        if (imageUrl is not null && !IsValidImage(imageUrl))
        {
            ModelState.AddModelError(nameof(Product.ImageUrl), "Tệp tải lên phải là hình ảnh hợp lệ.");
        }

        if (ModelState.IsValid)
        {
            var existingProduct = await _productRepository.GetByIdAsync(id);

            if (existingProduct is null)
            {
                return NotFound();
            }

            existingProduct.Name = product.Name;
            existingProduct.Price = product.Price;
            existingProduct.Description = product.Description;
            existingProduct.CategoryId = product.CategoryId;

            if (imageUrl is not null)
            {
                existingProduct.ImageUrl = await SaveImageAsync(imageUrl);
            }

            await _productRepository.UpdateAsync(existingProduct);
            return RedirectToAction(nameof(Index));
        }

        await LoadCategoriesAsync(product.CategoryId);
        return View(product);
    }

    public async Task<IActionResult> Delete(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);

        if (product is null)
        {
            return NotFound();
        }

        return View(product);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _productRepository.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadCategoriesAsync(int? selectedCategoryId = null)
    {
        var categories = await _categoryRepository.GetAllAsync();
        ViewBag.Categories = new SelectList(categories, "Id", "Name", selectedCategoryId);
    }

    private async Task<string> SaveImageAsync(IFormFile image)
    {
        var imagesDirectory = Path.Combine(_webHostEnvironment.WebRootPath, "images");
        Directory.CreateDirectory(imagesDirectory);

        var extension = Path.GetExtension(image.FileName);
        var fileName = $"{Guid.NewGuid():N}{extension}";
        var savePath = Path.Combine(imagesDirectory, fileName);

        await using var fileStream = new FileStream(savePath, FileMode.Create);
        await image.CopyToAsync(fileStream);

        return $"/images/{fileName}";
    }

    private static bool IsValidImage(IFormFile image)
    {
        var extension = Path.GetExtension(image.FileName);
        return image.Length > 0
            && image.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase)
            && AllowedImageExtensions.Contains(extension);
    }
}

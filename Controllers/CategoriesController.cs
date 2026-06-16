using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NguyenHaiDang_W345.Models;
using NguyenHaiDang_W345.Repositories;

namespace NguyenHaiDang_W345.Controllers;

[Authorize(Roles = SD.Role_Admin)]
public class CategoriesController(ICategoryRepository categoryRepository) : Controller
{
    public async Task<IActionResult> Index() => View(await categoryRepository.GetAllAsync());

    public async Task<IActionResult> Display(int id)
    {
        var category = await categoryRepository.GetByIdAsync(id);
        return category is null ? NotFound() : View(category);
    }

    public IActionResult Add() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(Category category)
    {
        if (!ModelState.IsValid) return View(category);

        await categoryRepository.AddAsync(category);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Update(int id)
    {
        var category = await categoryRepository.GetByIdAsync(id);
        return category is null ? NotFound() : View(category);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(int id, Category category)
    {
        if (id != category.Id) return NotFound();
        if (!ModelState.IsValid) return View(category);

        await categoryRepository.UpdateAsync(category);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var category = await categoryRepository.GetByIdAsync(id);
        return category is null ? NotFound() : View(category);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var category = await categoryRepository.GetByIdAsync(id);
        if (category is null) return NotFound();

        if (category.Products.Count > 0)
        {
            ModelState.AddModelError(string.Empty, "Cannot delete a category that contains products.");
            return View("Delete", category);
        }

        await categoryRepository.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }
}

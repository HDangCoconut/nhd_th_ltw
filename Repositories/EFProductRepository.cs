using Microsoft.EntityFrameworkCore;
using NguyenHaiDang_W345.Models;

namespace NguyenHaiDang_W345.Repositories;

public class EFProductRepository : IProductRepository
{
    private readonly ApplicationDbContext _context;

    public EFProductRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        return await _context.Products
            .Include(product => product.Category)
            .ToListAsync();
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        return await _context.Products
            .Include(product => product.Category)
            .Include(product => product.Images)
            .FirstOrDefaultAsync(product => product.Id == id);
    }

    public async Task AddAsync(Product product)
    {
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Product product)
    {
        _context.Products.Update(product);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);

        if (product is null)
        {
            return;
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
    }

    public async Task AddProductImagesAsync(int productId, IEnumerable<string> urls)
    {
        var images = urls.Select(url => new ProductImage
        {
            ProductId = productId,
            Url = url
        });

        await _context.ProductImages.AddRangeAsync(images);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteProductImagesAsync(IEnumerable<int> imageIds)
    {
        var ids = imageIds.ToList();
        if (ids.Count == 0)
        {
            return;
        }

        var images = await _context.ProductImages
            .Where(image => ids.Contains(image.Id))
            .ToListAsync();

        _context.ProductImages.RemoveRange(images);
        await _context.SaveChangesAsync();
    }
}

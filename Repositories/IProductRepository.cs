using NguyenHaiDang_W345.Models;

namespace NguyenHaiDang_W345.Repositories;

public interface IProductRepository
{
    Task<IEnumerable<Product>> GetAllAsync();
    Task<Product?> GetByIdAsync(int id);
    Task AddAsync(Product product);
    Task UpdateAsync(Product product);
    Task DeleteAsync(int id);
    Task AddProductImagesAsync(int productId, IEnumerable<string> urls);
    Task DeleteProductImagesAsync(IEnumerable<int> imageIds);
}

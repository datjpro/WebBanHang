public interface IProductImageRepository
{
    Task<IEnumerable<ProductImage>> GetAllAsync();
    Task<ProductImage> GetByIdAsync(int id);
    Task<IEnumerable<ProductImage>> GetByProductIdAsync(int productId);
    Task AddAsync(ProductImage productImage);
    Task AddRangeAsync(IEnumerable<ProductImage> productImages);
    Task UpdateAsync(ProductImage productImage);
    Task DeleteAsync(int id);
    Task DeleteByProductIdAsync(int productId);
}

using Microsoft.EntityFrameworkCore;

public class EFProductRepository : IProductRepository
{
 private readonly ApplicationDbContext _context;
 public EFProductRepository(ApplicationDbContext context)
 {
 _context = context;
 } public async Task<IEnumerable<Product>> GetAllAsync()
 {
 return await _context.Products
 .Include(p => p.Category) // Include thông tin về category
 .Include(p => p.Images) // Include thông tin về images
 .ToListAsync();
 } public async Task<Product> GetByIdAsync(int id)
 {
 // lấy thông tin kèm theo category và images
 return await _context.Products
 .Include(p => p.Category)
 .Include(p => p.Images)
 .FirstOrDefaultAsync(p => p.Id == id) ?? new Product();
 }
 public async Task AddAsync(Product product)
 {
 _context.Products.Add(product);
 await _context.SaveChangesAsync();
 }
 public async Task UpdateAsync(Product product)
 {
 _context.Products.Update(product);
 await _context.SaveChangesAsync();
 } public async Task DeleteAsync(int id)
 {
 var product = await _context.Products.FindAsync(id);
 if (product != null)
 {
 _context.Products.Remove(product);
 await _context.SaveChangesAsync();
 }
 }

 public async Task<IEnumerable<Product>> GetByCategoryIdAsync(int categoryId)
 {
 return await _context.Products
 .Include(p => p.Category)
 .Include(p => p.Images)
 .Where(p => p.CategoryId == categoryId)
 .ToListAsync();
 }

 public async Task<IEnumerable<Product>> SearchAsync(string searchString)
 {
 return await _context.Products
 .Include(p => p.Category)
 .Include(p => p.Images)
 .Where(p => p.Name.Contains(searchString))
 .ToListAsync();
 }

 public async Task<IEnumerable<Product>> GetByCategoryAndSearchAsync(int? categoryId, string? searchString)
 {
 var products = _context.Products
 .Include(p => p.Category)
 .Include(p => p.Images)
 .AsQueryable();

 if (categoryId.HasValue)
 {
 products = products.Where(p => p.CategoryId == categoryId);
 }

 if (!string.IsNullOrEmpty(searchString))
 {
 products = products.Where(p => p.Name.Contains(searchString));
 }

 return await products.ToListAsync();
 }
}
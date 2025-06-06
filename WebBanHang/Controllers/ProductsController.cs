using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using WebBanHang.Models;

namespace WebBanHang.Controllers
{
    public class ProductsController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IProductImageRepository _productImageRepository;

        public ProductsController(IProductRepository productRepository, 
                                ICategoryRepository categoryRepository, 
                                IProductImageRepository productImageRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _productImageRepository = productImageRepository;
        }        // GET: Products
        public async Task<IActionResult> Index(int? categoryId, string? searchString)
        {
            var products = await _productRepository.GetByCategoryAndSearchAsync(categoryId, searchString);
            var categories = await _categoryRepository.GetAllAsync();

            ViewBag.Categories = categories;
            ViewBag.CurrentCategory = categoryId;
            ViewBag.SearchString = searchString;

            return View(products);
        }        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _productRepository.GetByIdAsync(id.Value);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);        }        // GET: Products/Create
        [Authorize(Policy = "AdminOrCompany")]
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _categoryRepository.GetAllAsync();
            return View();
        }        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "AdminOrCompany")]
        public async Task<IActionResult> Create([Bind("Id,Name,Price,Description,ImageUrl,CategoryId")] Product product, List<string> imageUrls)
        {
            // Debug: Log các giá trị nhận được
            Console.WriteLine($"Product: {product.Name}, ImageUrl: {product.ImageUrl}");
            Console.WriteLine($"ImageUrls count: {imageUrls?.Count ?? 0}");
            if (imageUrls != null)
            {
                foreach (var url in imageUrls)
                {
                    Console.WriteLine($"ImageUrl: {url}");
                }
            }

            if (ModelState.IsValid)
            {
                // Thêm sản phẩm trước
                await _productRepository.AddAsync(product);
                Console.WriteLine($"Product added with ID: {product.Id}");

                // Nếu có imageUrls, thêm vào bảng ProductImages
                if (imageUrls != null && imageUrls.Any())
                {
                    var productImages = imageUrls.Where(url => !string.IsNullOrEmpty(url))
                                                .Select(url => new ProductImage
                                                {
                                                    Url = url,
                                                    ProductId = product.Id
                                                });
                    
                    Console.WriteLine($"Adding {productImages.Count()} images to ProductImages table");
                    await _productImageRepository.AddRangeAsync(productImages);
                    Console.WriteLine("Images added successfully");
                }

                return RedirectToAction(nameof(Index));
            }
            ViewBag.Categories = await _categoryRepository.GetAllAsync();
            return View(product);        }// GET: Products/Edit/5
        [Authorize(Policy = "AdminOrCompany")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _productRepository.GetByIdAsync(id.Value);
            if (product == null)
            {
                return NotFound();
            }
            ViewBag.Categories = await _categoryRepository.GetAllAsync();
            return View(product);        }        // POST: Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "AdminOrCompany")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Price,Description,ImageUrl,CategoryId")] Product product, List<string> imageUrls)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Cập nhật sản phẩm
                    await _productRepository.UpdateAsync(product);

                    // Xóa tất cả ảnh cũ và thêm ảnh mới
                    await _productImageRepository.DeleteByProductIdAsync(product.Id);
                    
                    if (imageUrls != null && imageUrls.Any())
                    {
                        var productImages = imageUrls.Where(url => !string.IsNullOrEmpty(url))
                                                    .Select(url => new ProductImage
                                                    {
                                                        Url = url,
                                                        ProductId = product.Id
                                                    });
                        
                        await _productImageRepository.AddRangeAsync(productImages);
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await ProductExistsAsync(product.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Categories = await _categoryRepository.GetAllAsync();
            return View(product);        }        // GET: Products/Delete/5
        [Authorize(Policy = "AdminOrCompany")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _productRepository.GetByIdAsync(id.Value);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);        }        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "AdminOrCompany")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Xóa tất cả ảnh liên quan trước
            await _productImageRepository.DeleteByProductIdAsync(id);
            
            // Sau đó xóa sản phẩm
            await _productRepository.DeleteAsync(id);

            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> ProductExistsAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            return product != null;
        }
    }
}

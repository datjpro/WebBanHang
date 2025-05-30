using Microsoft.AspNetCore.Mvc;

namespace WebBanHang.Controllers
{
    public class ProductImagesController : Controller
    {
        private readonly IProductImageRepository _productImageRepository;
        private readonly IProductRepository _productRepository;

        public ProductImagesController(IProductImageRepository productImageRepository, IProductRepository productRepository)
        {
            _productImageRepository = productImageRepository;
            _productRepository = productRepository;
        }

        // GET: ProductImages/Create/5
        public async Task<IActionResult> Create(int productId)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
            {
                return NotFound();
            }

            ViewBag.Product = product;
            return View();
        }

        // POST: ProductImages/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Url,ProductId")] ProductImage productImage)
        {
            if (ModelState.IsValid)
            {
                await _productImageRepository.AddAsync(productImage);
                return RedirectToAction("Details", "Products", new { id = productImage.ProductId });
            }

            var product = await _productRepository.GetByIdAsync(productImage.ProductId);
            ViewBag.Product = product;
            return View(productImage);
        }

        // GET: ProductImages/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productImage = await _productImageRepository.GetByIdAsync(id.Value);
            if (productImage == null)
            {
                return NotFound();
            }

            return View(productImage);
        }

        // POST: ProductImages/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var productImage = await _productImageRepository.GetByIdAsync(id);
            if (productImage != null)
            {
                var productId = productImage.ProductId;
                await _productImageRepository.DeleteAsync(id);
                return RedirectToAction("Details", "Products", new { id = productId });
            }

            return RedirectToAction("Index", "Products");
        }
    }
}

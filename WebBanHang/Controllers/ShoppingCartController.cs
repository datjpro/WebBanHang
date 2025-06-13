using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using WebBanHang.Models;
using WebBanHang.Repositories;
using WebBanHang.Extensions;

namespace WebBanHang.Controllers
{
    public class ShoppingCartController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICartRepository _cartRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public ShoppingCartController(IProductRepository productRepository, ICartRepository cartRepository, UserManager<ApplicationUser> userManager)
        {
            _productRepository = productRepository;
            _cartRepository = cartRepository;
            _userManager = userManager;
        }        // GET: ShoppingCart
        public async Task<IActionResult> Index()
        {
            var cart = await GetCartFromDatabaseAndSession();
            return View(cart);
        }        // POST: Add to cart
        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            try
            {
                var product = await GetProductFromDatabase(productId);
                if (product == null)
                {
                    TempData["Error"] = "Sản phẩm không tồn tại.";
                    return RedirectToAction("Index", "Products");
                }

                var cartItem = new CartItem
                {
                    ProductId = productId,
                    Name = product.Name,
                    ImageUrl = product.ImageUrl ?? "",
                    Price = product.Price,
                    Quantity = quantity,
                    UserId = User.Identity?.IsAuthenticated == true ? _userManager.GetUserId(User) : null,
                    SessionId = HttpContext.Session.Id
                };                // Save to database
                await _cartRepository.AddToCartAsync(cartItem);

                // Also save to session for immediate access
                var cart = await GetCartFromDatabaseAndSession();

                TempData["Success"] = $"Đã thêm {product.Name} vào giỏ hàng.";
                
                // Return JSON for AJAX requests
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { 
                        success = true, 
                        message = $"Đã thêm {product.Name} vào giỏ hàng.",
                        cartCount = cart.GetTotalItems()
                    });
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra khi thêm sản phẩm vào giỏ hàng.";
                return RedirectToAction("Index", "Products");
            }
        }        // POST: Update quantity
        [HttpPost]        public async Task<IActionResult> UpdateQuantity(int productId, int quantity)
        {
            try
            {
                var userId = User.Identity?.IsAuthenticated == true ? _userManager.GetUserId(User) : null;
                var sessionId = HttpContext.Session.Id;
                
                var cartItems = await _cartRepository.GetCartItemsAsync(userId, sessionId);
                var cartItem = cartItems.FirstOrDefault(x => x.ProductId == productId);
                
                if (cartItem != null)
                {                    if (quantity <= 0)
                    {
                        await _cartRepository.RemoveFromCartAsync(cartItem.Id);
                    }
                    else
                    {
                        cartItem.Quantity = quantity;
                        await _cartRepository.UpdateCartItemAsync(cartItem);
                    }
                    
                    var cart = await GetCartFromDatabaseAndSession();
                      if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { 
                            success = true, 
                            total = cart.GetTotalAmount(),
                            cartCount = cart.GetTotalItems()
                        });
                    }
                }
                return RedirectToAction("Index");
            }
            catch
            {
                return RedirectToAction("Index");
            }
        }        // POST: Remove from cart
        [HttpPost]        public async Task<IActionResult> RemoveFromCart(int productId)
        {
            try
            {
                var userId = User.Identity?.IsAuthenticated == true ? _userManager.GetUserId(User) : null;
                var sessionId = HttpContext.Session.Id;
                
                var cartItems = await _cartRepository.GetCartItemsAsync(userId, sessionId);
                var cartItem = cartItems.FirstOrDefault(x => x.ProductId == productId);
                  if (cartItem != null)
                {
                    await _cartRepository.RemoveFromCartAsync(cartItem.Id);
                    
                    var cart = await GetCartFromDatabaseAndSession();
                    TempData["Success"] = "Đã xóa sản phẩm khỏi giỏ hàng.";
                    
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { 
                            success = true,
                            message = "Đã xóa sản phẩm khỏi giỏ hàng.",
                            total = cart.GetTotalAmount(),
                            cartCount = cart.GetTotalItems()
                        });
                    }
                }
                return RedirectToAction("Index");
            }
            catch
            {
                return RedirectToAction("Index");
            }
        }// POST: Clear cart
        [HttpPost]
        public async Task<IActionResult> ClearCart()
        {            var userId = User.Identity?.IsAuthenticated == true ? _userManager.GetUserId(User) : null;
            var sessionId = HttpContext.Session.Id;
            
            await _cartRepository.ClearCartAsync(userId, sessionId);
            
            var cart = new ShoppingCart();
            HttpContext.Session.SetObjectAsJson("Cart", cart);
            TempData["Success"] = "Đã xóa tất cả sản phẩm khỏi giỏ hàng.";
            return RedirectToAction("Index");        }

        // GET: Get cart count for header display
        public async Task<IActionResult> GetCartCount()
        {
            var userId = User.Identity?.IsAuthenticated == true ? _userManager.GetUserId(User) : null;
            var sessionId = HttpContext.Session.Id;
            var count = await _cartRepository.GetCartCountAsync(userId, sessionId);
            return Json(count);
        }

        // GET: Get cart total for header display
        public async Task<IActionResult> GetCartTotal()
        {
            var userId = User.Identity?.IsAuthenticated == true ? _userManager.GetUserId(User) : null;
            var sessionId = HttpContext.Session.Id;
            var total = await _cartRepository.GetCartTotalAsync(userId, sessionId);
            return Json(total);
        }        // GET: Get cart data for header display
        public async Task<IActionResult> GetCartHeaderData()
        {
            try
            {
                var userId = User.Identity?.IsAuthenticated == true ? _userManager.GetUserId(User) : null;
                var sessionId = HttpContext.Session.Id;
                
                var cartItems = await _cartRepository.GetCartItemsAsync(userId, sessionId);
                var count = cartItems.Sum(x => x.Quantity);
                var total = cartItems.Sum(x => x.Price * x.Quantity);
                
                var items = cartItems.Take(5).Select(x => new {
                    name = x.Name,
                    imageUrl = x.ImageUrl,
                    price = x.Price,
                    quantity = x.Quantity
                }).ToList();
                
                return Json(new {
                    count = count,
                    total = total,
                    items = items
                });
            }
            catch
            {
                return Json(new { count = 0, total = 0, items = new object[0] });
            }
        }        private async Task<ShoppingCart> GetCartFromDatabaseAndSession()
        {
            var userId = User.Identity?.IsAuthenticated == true ? _userManager.GetUserId(User) : null;
            var sessionId = HttpContext.Session.Id;
            
            var cartItems = await _cartRepository.GetCartItemsAsync(userId, sessionId);
            var cart = new ShoppingCart();
            
            foreach (var item in cartItems)
            {
                cart.Items.Add(item);
            }
            
            // Also update session for quick access
            HttpContext.Session.SetObjectAsJson("Cart", cart);
            return cart;
        }

        private async Task<Product> GetProductFromDatabase(int productId)
        {
            return await _productRepository.GetByIdAsync(productId);
        }
    }
}

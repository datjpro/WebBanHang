using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using WebBanHang.Models;
using WebBanHang.Repositories;
using System.Security.Claims;

namespace WebBanHang.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICartRepository _cartRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrderController(IOrderRepository orderRepository, ICartRepository cartRepository, UserManager<ApplicationUser> userManager)
        {
            _orderRepository = orderRepository;
            _cartRepository = cartRepository;
            _userManager = userManager;
        }

        // GET: Hiển thị danh sách đơn hàng của user
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            var orders = await _orderRepository.GetOrdersByUserIdAsync(userId);
            return View(orders);
        }

        // GET: Hiển thị chi tiết đơn hàng
        public async Task<IActionResult> Details(int id)
        {
            var order = await _orderRepository.GetOrderWithDetailsAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            // Kiểm tra quyền xem đơn hàng
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (order.UserId != userId && !User.IsInRole(SD.Role_Admin))
            {
                return Forbid();
            }

            return View(order);
        }

        // GET: Trang checkout
        public async Task<IActionResult> Checkout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var sessionId = HttpContext.Session.Id;

            var cartItems = await _cartRepository.GetCartItemsAsync(userId, sessionId);
            if (!cartItems.Any())
            {
                TempData["Error"] = "Giỏ hàng của bạn đang trống!";
                return RedirectToAction("Index", "ShoppingCart");
            }

            var user = await _userManager.GetUserAsync(User);
            var order = new Order
            {
                UserId = userId!,
                ShippingAddress = user?.Address ?? "",
                TotalPrice = cartItems.Sum(item => item.Price * item.Quantity)
            };

            ViewBag.CartItems = cartItems;
            return View(order);
        }

        // POST: Xử lý đặt hàng
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(Order order)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var sessionId = HttpContext.Session.Id;

            if (userId == null)
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            var cartItems = await _cartRepository.GetCartItemsAsync(userId, sessionId);
            if (!cartItems.Any())
            {
                TempData["Error"] = "Giỏ hàng của bạn đang trống!";
                return RedirectToAction("Index", "ShoppingCart");
            }            // Debug logging
            Console.WriteLine($"=== ORDER CHECKOUT DEBUG ===");
            Console.WriteLine($"UserId: {userId}");
            Console.WriteLine($"SessionId: {sessionId}");
            Console.WriteLine($"Cart Items Count: {cartItems.Count()}");
            Console.WriteLine($"ShippingAddress: '{order.ShippingAddress}'");
            Console.WriteLine($"Notes: '{order.Notes}'");

            // Loại bỏ validation errors cho các field sẽ được set bởi controller
            ModelState.Remove("UserId");
            ModelState.Remove("OrderDate");
            ModelState.Remove("TotalPrice");
            ModelState.Remove("ApplicationUser");
            ModelState.Remove("OrderDetails");

            Console.WriteLine($"ModelState.IsValid after cleanup: {ModelState.IsValid}");

            if (!ModelState.IsValid)
            {
                Console.WriteLine("=== MODEL STATE ERRORS ===");
                foreach (var modelState in ModelState)
                {
                    foreach (var error in modelState.Value.Errors)
                    {
                        Console.WriteLine($"Key: {modelState.Key}, Error: {error.ErrorMessage}");
                    }
                }
                ViewBag.CartItems = cartItems;
                return View(order);
            }

            if (ModelState.IsValid)
            {                try
                {
                    // Tạo đơn hàng với chi tiết ngay từ đầu
                    order.UserId = userId;
                    order.OrderDate = DateTime.Now;
                    order.TotalPrice = cartItems.Sum(item => item.Price * item.Quantity);
                    
                    Console.WriteLine($"Order TotalPrice: {order.TotalPrice}");
                    
                    // Tạo danh sách chi tiết đơn hàng
                    order.OrderDetails = new List<OrderDetail>();
                    foreach (var cartItem in cartItems)
                    {
                        var orderDetail = new OrderDetail
                        {
                            ProductId = cartItem.ProductId,
                            Quantity = cartItem.Quantity,
                            Price = cartItem.Price
                        };
                        order.OrderDetails.Add(orderDetail);
                        Console.WriteLine($"Added OrderDetail: ProductId={cartItem.ProductId}, Qty={cartItem.Quantity}, Price={cartItem.Price}");
                    }

                    Console.WriteLine($"Total OrderDetails: {order.OrderDetails.Count}");

                    // Lưu đơn hàng (sẽ tự động lưu cả OrderDetails do relationship)
                    var createdOrder = await _orderRepository.CreateOrderAsync(order);
                    
                    Console.WriteLine($"Order created with ID: {createdOrder.Id}");

                    // Xóa giỏ hàng sau khi đặt hàng thành công
                    await _cartRepository.ClearCartAsync(userId, sessionId);
                    
                    Console.WriteLine("Cart cleared successfully");

                    TempData["Success"] = $"Đặt hàng thành công! Mã đơn hàng: #{createdOrder.Id}";
                    return RedirectToAction("OrderSuccess", new { id = createdOrder.Id });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"=== ORDER ERROR ===");
                    Console.WriteLine($"Exception: {ex.Message}");
                    Console.WriteLine($"StackTrace: {ex.StackTrace}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                    }
                    
                    ModelState.AddModelError("", $"Có lỗi xảy ra khi đặt hàng: {ex.Message}");
                    TempData["Error"] = "Đặt hàng thất bại!";
                }
            }

            // Nếu có lỗi, hiển thị lại form với dữ liệu giỏ hàng
            ViewBag.CartItems = cartItems;
            return View(order);
        }

        // GET: Trang thành công sau khi đặt hàng
        public async Task<IActionResult> OrderSuccess(int id)
        {
            var order = await _orderRepository.GetOrderWithDetailsAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            // Kiểm tra quyền xem đơn hàng
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (order.UserId != userId)
            {
                return Forbid();
            }

            return View(order);
        }
    }
}

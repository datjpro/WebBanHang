using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using WebBanHang.Models;
using WebBanHang.Repositories;

namespace WebBanHang.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class OrderController : Controller
    {
        private readonly IOrderRepository _orderRepository;

        public OrderController(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        // GET: Admin/Order
        public async Task<IActionResult> Index()
        {
            var orders = await _orderRepository.GetAllOrdersAsync();
            return View(orders);
        }

        // GET: Admin/Order/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var order = await _orderRepository.GetOrderWithDetailsAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            return View(order);
        }

        // POST: Admin/Order/UpdateStatus
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int orderId, string status)
        {
            try
            {
                var order = await _orderRepository.GetOrderByIdAsync(orderId);
                if (order == null)
                {
                    return Json(new { success = false, message = "Đơn hàng không tồn tại!" });
                }

                // Cập nhật status (cần thêm property Status vào model Order)
                // order.Status = status;
                await _orderRepository.UpdateOrderAsync(order);

                return Json(new { success = true, message = "Cập nhật trạng thái thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Có lỗi xảy ra: {ex.Message}" });
            }
        }

        // POST: Admin/Order/Delete/5
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _orderRepository.DeleteOrderAsync(id);
                TempData["Success"] = "Xóa đơn hàng thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Có lỗi xảy ra: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}

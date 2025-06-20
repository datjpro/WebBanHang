using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanHang.Models;
using WebBanHang.Repositories;

namespace WebBanHang.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IOrderRepository _orderRepository;

        public HomeController(ApplicationDbContext context, IOrderRepository orderRepository)
        {
            _context = context;
            _orderRepository = orderRepository;
        }

        public async Task<IActionResult> Index()
        {
            // Thống kê cho trang Admin Dashboard
            ViewBag.TotalProducts = await _context.Products.CountAsync();
            ViewBag.TotalCategories = await _context.Categories.CountAsync();
            ViewBag.TotalUsers = await _context.Users.CountAsync();
            
            // Thống kê đơn hàng
            var orders = await _orderRepository.GetAllOrdersAsync();
            ViewBag.TotalOrders = orders.Count();
            ViewBag.TodayOrders = orders.Count(o => o.OrderDate.Date == DateTime.Today);
            ViewBag.TotalRevenue = orders.Sum(o => o.TotalPrice);
            
            // Sản phẩm mới nhất
            var latestProducts = await _context.Products
                .Include(p => p.Category)
                .OrderByDescending(p => p.Id)
                .Take(5)
                .ToListAsync();

            return View(latestProducts);
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using WebBanHang.Models;
using WebBanHang.Repositories;

namespace WebBanHang.ViewComponents
{
    public class CartHeaderViewComponent : ViewComponent
    {
        private readonly ICartRepository _cartRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public CartHeaderViewComponent(ICartRepository cartRepository, UserManager<ApplicationUser> userManager)
        {
            _cartRepository = cartRepository;
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            try
            {
                var userId = UserClaimsPrincipal.Identity?.IsAuthenticated == true ? _userManager.GetUserId(UserClaimsPrincipal) : null;
                var sessionId = HttpContext.Session.Id;
                
                var cartItems = await _cartRepository.GetCartItemsAsync(userId, sessionId);
                var count = cartItems.Sum(x => x.Quantity);
                var total = cartItems.Sum(x => x.Price * x.Quantity);
                
                var items = cartItems.Take(5).Select(x => new {
                    Name = x.Name,
                    ImageUrl = x.ImageUrl,
                    Price = x.Price,
                    Quantity = x.Quantity
                }).ToList();
                
                ViewBag.Count = count;
                ViewBag.Total = total;
                ViewBag.Items = items;
                
                return View();
            }
            catch
            {
                ViewBag.Count = 0;
                ViewBag.Total = 0;
                ViewBag.Items = new object[0];
                return View();
            }
        }
    }
}

using WebBanHang.Models;

namespace WebBanHang.Repositories
{
    public interface IOrderRepository
    {
        Task<IEnumerable<Order>> GetAllOrdersAsync();
        Task<IEnumerable<Order>> GetOrdersByUserIdAsync(string userId);
        Task<Order?> GetOrderByIdAsync(int id);
        Task<Order?> GetOrderWithDetailsAsync(int id);
        Task<Order> CreateOrderAsync(Order order);
        Task<Order> UpdateOrderAsync(Order order);
        Task DeleteOrderAsync(int id);
        Task<bool> OrderExistsAsync(int id);
    }
}

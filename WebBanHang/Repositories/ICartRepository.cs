using WebBanHang.Models;

namespace WebBanHang.Repositories
{
    public interface ICartRepository
    {
        Task<IEnumerable<CartItem>> GetCartItemsAsync(string? userId, string? sessionId);
        Task<CartItem?> GetCartItemAsync(int productId, string? userId, string? sessionId);
        Task AddToCartAsync(CartItem cartItem);
        Task UpdateCartItemAsync(CartItem cartItem);
        Task RemoveFromCartAsync(int cartItemId);
        Task ClearCartAsync(string? userId, string? sessionId);
        Task<int> GetCartCountAsync(string? userId, string? sessionId);
        Task<decimal> GetCartTotalAsync(string? userId, string? sessionId);
        Task MergeCartAsync(string sessionId, string userId);
    }
}

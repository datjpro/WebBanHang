using Microsoft.EntityFrameworkCore;
using WebBanHang.Models;

namespace WebBanHang.Repositories
{
    public class EFCartRepository : ICartRepository
    {
        private readonly ApplicationDbContext _context;

        public EFCartRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CartItem>> GetCartItemsAsync(string? userId, string? sessionId)
        {
            var query = _context.CartItems.Include(c => c.Product).AsQueryable();

            if (!string.IsNullOrEmpty(userId))
            {
                query = query.Where(c => c.UserId == userId);
            }
            else if (!string.IsNullOrEmpty(sessionId))
            {
                query = query.Where(c => c.SessionId == sessionId && c.UserId == null);
            }
            else
            {
                return new List<CartItem>();
            }

            return await query.OrderBy(c => c.CreatedAt).ToListAsync();
        }

        public async Task<CartItem?> GetCartItemAsync(int productId, string? userId, string? sessionId)
        {
            var query = _context.CartItems.AsQueryable();

            if (!string.IsNullOrEmpty(userId))
            {
                query = query.Where(c => c.ProductId == productId && c.UserId == userId);
            }
            else if (!string.IsNullOrEmpty(sessionId))
            {
                query = query.Where(c => c.ProductId == productId && c.SessionId == sessionId && c.UserId == null);
            }
            else
            {
                return null;
            }

            return await query.FirstOrDefaultAsync();
        }

        public async Task AddToCartAsync(CartItem cartItem)
        {
            var existingItem = await GetCartItemAsync(cartItem.ProductId, cartItem.UserId, cartItem.SessionId);

            if (existingItem != null)
            {
                existingItem.Quantity += cartItem.Quantity;
                _context.CartItems.Update(existingItem);
            }
            else
            {
                _context.CartItems.Add(cartItem);
            }

            await _context.SaveChangesAsync();
        }

        public async Task UpdateCartItemAsync(CartItem cartItem)
        {
            _context.CartItems.Update(cartItem);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveFromCartAsync(int cartItemId)
        {
            var cartItem = await _context.CartItems.FindAsync(cartItemId);
            if (cartItem != null)
            {
                _context.CartItems.Remove(cartItem);
                await _context.SaveChangesAsync();
            }
        }

        public async Task ClearCartAsync(string? userId, string? sessionId)
        {
            var query = _context.CartItems.AsQueryable();

            if (!string.IsNullOrEmpty(userId))
            {
                query = query.Where(c => c.UserId == userId);
            }
            else if (!string.IsNullOrEmpty(sessionId))
            {
                query = query.Where(c => c.SessionId == sessionId && c.UserId == null);
            }

            var items = await query.ToListAsync();
            _context.CartItems.RemoveRange(items);
            await _context.SaveChangesAsync();
        }

        public async Task<int> GetCartCountAsync(string? userId, string? sessionId)
        {
            var query = _context.CartItems.AsQueryable();

            if (!string.IsNullOrEmpty(userId))
            {
                query = query.Where(c => c.UserId == userId);
            }
            else if (!string.IsNullOrEmpty(sessionId))
            {
                query = query.Where(c => c.SessionId == sessionId && c.UserId == null);
            }
            else
            {
                return 0;
            }

            return await query.SumAsync(c => c.Quantity);
        }

        public async Task<decimal> GetCartTotalAsync(string? userId, string? sessionId)
        {
            var query = _context.CartItems.AsQueryable();

            if (!string.IsNullOrEmpty(userId))
            {
                query = query.Where(c => c.UserId == userId);
            }
            else if (!string.IsNullOrEmpty(sessionId))
            {
                query = query.Where(c => c.SessionId == sessionId && c.UserId == null);
            }
            else
            {
                return 0;
            }

            return await query.SumAsync(c => c.Price * c.Quantity);
        }

        public async Task MergeCartAsync(string sessionId, string userId)
        {
            // Get session cart items
            var sessionItems = await _context.CartItems
                .Where(c => c.SessionId == sessionId && c.UserId == null)
                .ToListAsync();

            foreach (var sessionItem in sessionItems)
            {
                // Check if user already has this product in cart
                var userItem = await _context.CartItems
                    .FirstOrDefaultAsync(c => c.ProductId == sessionItem.ProductId && c.UserId == userId);

                if (userItem != null)
                {
                    // Merge quantities
                    userItem.Quantity += sessionItem.Quantity;
                    _context.CartItems.Update(userItem);
                }
                else
                {
                    // Transfer session item to user
                    sessionItem.UserId = userId;
                    sessionItem.SessionId = null;
                    _context.CartItems.Update(sessionItem);
                }
            }

            // Remove any remaining session items
            var remainingSessionItems = await _context.CartItems
                .Where(c => c.SessionId == sessionId && c.UserId == null)
                .ToListAsync();
            
            _context.CartItems.RemoveRange(remainingSessionItems);

            await _context.SaveChangesAsync();
        }
    }
}

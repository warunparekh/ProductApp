using Dapper;
using Dapper.Contrib.Extensions;
using ProductApp.Models;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ProductApp.Repositories
{
    public class CartRepository
    {
        private readonly IDbConnection _db;
        private const string GetCartFields = "SELECT CartId, UserId, ProductId, ProductQuantity FROM Cart";

        public CartRepository(IDbConnection db)
        {
            _db = db;
        }

        // This is the single, robust method for getting cart data.
        private async Task<IEnumerable<Cart>> GetAllCartsFromDbAsync()
        {
            return await _db.QueryAsync<Cart>(GetCartFields);
        }

        public async Task<IEnumerable<Cart>> GetByUserIdAsync(string userId)
        {
            var allCarts = await GetAllCartsFromDbAsync();
            return allCarts.Where(c => c.UserId == userId);
        }

        public async Task AddAsync(Cart cart)
        {
            await _db.InsertAsync(cart);
        }

        public async Task UpdateAsync(Cart cart)
        {
            await _db.UpdateAsync(cart);
        }

        public async Task DeleteAsync(Cart cart)
        {
            await _db.DeleteAsync(cart);
        }

        public async Task<Cart> GetCartItemAsync(int productId, string userId)
        {
            var userCart = await GetByUserIdAsync(userId);
            return userCart.FirstOrDefault(c => c.ProductId == productId);
        }

        public async Task<bool> IsProductInCartAsync(int productId, string userId)
        {
            var item = await GetCartItemAsync(productId, userId);
            return item != null;
        }

        public async Task ClearCartAsync(string userId)
        {
            var userCarts = await GetByUserIdAsync(userId);
            foreach (var cart in userCarts)
            {
                await _db.DeleteAsync(cart);
            }
        }

        public async Task<int> GetCartItemCountAsync(string userId)
        {
            var userCarts = await GetByUserIdAsync(userId);
            return userCarts.Count();
        }

        public async Task<IEnumerable<Cart>> GetAllAsync()
        {
            return await GetAllCartsFromDbAsync();
        }
    }
}
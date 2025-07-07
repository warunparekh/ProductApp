using Dapper.Contrib.Extensions;
using ProductApp.Models;
using System.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Dapper; // Add this using statement

namespace ProductApp.Repositories
{
    public class CartRepository
    {
        private readonly IDbConnection _db;
        public CartRepository(IDbConnection db)
        {
            _db = db;
        }

        public async Task<IEnumerable<Cart>> GetByUserIdAsync(string userId)
        {
            var all = await this.GetAllAsync(); // Use the new robust method
            return all.Where(c => c.UserId == userId);
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
            var all = await this.GetAllAsync(); // Use the new robust method
            return all.FirstOrDefault(c => c.ProductId == productId && c.UserId == userId);
        }

        public async Task<bool> IsProductInCartAsync(int productId, string userId)
        {
            var all = await this.GetAllAsync(); // Use the new robust method
            return all.Any(c => c.ProductId == productId && c.UserId == userId);
        }

        public async Task ClearCartAsync(string userId)
        {
            var all = await this.GetAllAsync(); // Use the new robust method
            var userCarts = all.Where(c => c.UserId == userId).ToList();
            foreach (var cart in userCarts)
            {
                await _db.DeleteAsync(cart);
            }
        }

        public async Task<int> GetCartItemCountAsync(string userId)
        {
            var all = await this.GetAllAsync(); // Use the new robust method
            return all.Count(c => c.UserId == userId);
        }

        // CORRECTED METHOD: Use a specific Dapper query instead of GetAllAsync
        public async Task<IEnumerable<Cart>> GetAllAsync()
        {
            var sql = "SELECT CartId, UserId, ProductId, ProductQuantity, ProductNetprice, ProductName, CartTotalPrice FROM Cart";
            return await _db.QueryAsync<Cart>(sql);
        }
    }
}
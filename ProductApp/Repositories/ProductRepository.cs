using Dapper;
using Dapper.Contrib.Extensions;
using ProductApp.Models;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ProductApp.Repositories
{
    public class ProductRepository
    {
        private readonly IDbConnection _db;
        public ProductRepository(IDbConnection db) => _db = db;

        public async Task<IEnumerable<ProductWithCategoryName>> GetAllWithCategoryNameAsync()
        {
            var sql = @"
                SELECT p.ProductId, p.ProductName, p.ProductDescription, p.ProductPrice, p.ProductStock, p.ProductImage, p.CategoryId,
                       c.CategoryName
                FROM Products p
                INNER JOIN Categories c ON p.CategoryId = c.CategoryId";
            return await _db.QueryAsync<ProductWithCategoryName>(sql);
        }

        public async Task<ProductWithCategoryName?> GetByIdWithCategoryNameAsync(int id)
        {
            var sql = @"
                SELECT p.ProductId, p.ProductName, p.ProductDescription, p.ProductPrice, p.ProductStock, p.ProductImage, p.CategoryId,
                       c.CategoryName
                FROM Products p
                INNER JOIN Categories c ON p.CategoryId = c.CategoryId
                WHERE p.ProductId = @Id";
            return await _db.QueryFirstOrDefaultAsync<ProductWithCategoryName>(sql, new { Id = id });
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
            => await _db.GetAllAsync<Product>();

        public async Task<Product> GetByIdAsync(int id)
            => await _db.GetAsync<Product>(id);

        public async Task AddAsync(Product p)
        {
            await _db.InsertAsync(p);
        }

        public async Task UpdateAsync(Product p)
        {
            await _db.UpdateAsync(p);
        }

        public async Task DeleteAsync(int id)
        {
            var product = await _db.GetAsync<Product>(id);
            if (product != null)
                await _db.DeleteAsync(product);
        }

        public async Task<IEnumerable<Product>> GetByIdsAsync(IEnumerable<int> productIds)
        {
            var all = await _db.GetAllAsync<Product>();
            return all.Where(p => productIds.Contains(p.ProductId));
        }
    }
}
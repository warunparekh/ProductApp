using Dapper;
using Dapper.Contrib.Extensions;
using ProductApp.Models;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace ProductApp.Repositories
{
    public class CategoryRepository
    {
        private readonly IDbConnection _db;
        public CategoryRepository(IDbConnection db) => _db = db;

        public async Task<IEnumerable<Category>> GetAllAsync()
            => await _db.GetAllAsync<Category>();

        public async Task<Category> GetByIdAsync(int id)
            => await _db.GetAsync<Category>(id);

        public async Task AddAsync(Category c)
            => await _db.InsertAsync(c);

        public async Task UpdateAsync(Category c)
            => await _db.UpdateAsync(c);

        public async Task DeleteAsync(int id)
        {
            var category = await _db.GetAsync<Category>(id);
            if (category != null)
                await _db.DeleteAsync(category);
        }

        // Dapper query for product count
        public async Task<IEnumerable<CategoryWithCount>> GetAllWithProductCountAsync()
        {
            var sql = @"
                SELECT c.CategoryId, c.CategoryName, COUNT(p.ProductId) AS ProductCount
                FROM Categories c
                LEFT JOIN Products p ON p.CategoryId = c.CategoryId
                GROUP BY c.CategoryId, c.CategoryName";
            return await _db.QueryAsync<CategoryWithCount>(sql);
        }
    }
}
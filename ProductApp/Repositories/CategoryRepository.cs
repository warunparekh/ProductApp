//using Dapper;
//using ProductApp.Models;
//using System.Collections.Generic;
//using System.Data;
//using System.Threading.Tasks;

//namespace ProductApp.Repositories
//{
//    public class CategoryRepository
//    {
//        private readonly IDbConnection _db;
//        public CategoryRepository(IDbConnection db) => _db = db;

//        public async Task<IEnumerable<Category>> GetAllAsync()
//        {
//            var sql = @"
//                SELECT c.CategoryId, c.CategoryName,
//                       COUNT(p.ProductId) AS ProductCount
//                FROM Categories c
//                LEFT JOIN Products p ON p.CategoryId = c.CategoryId
//                GROUP BY c.CategoryId, c.CategoryName";
//            return await _db.QueryAsync<Category>(sql);
//        }

//        public async Task<Category> GetByIdAsync(int id)
//        {
//            var sql = @"
//                SELECT c.CategoryId, c.CategoryName,
//                       COUNT(p.ProductId) AS ProductCount
//                FROM Categories c
//                LEFT JOIN Products p ON p.CategoryId = c.CategoryId
//                WHERE c.CategoryId = @Id
//                GROUP BY c.CategoryId, c.CategoryName";
//            return await _db.QueryFirstOrDefaultAsync<Category>(sql, new { Id = id });
//        }

//        public async Task AddAsync(Category c)
//        {
//            var sql = "INSERT INTO Categories (CategoryName) VALUES (@CategoryName)";
//            await _db.ExecuteAsync(sql, c);
//        }

//        public async Task UpdateAsync(Category c)
//        {
//            var sql = "UPDATE Categories SET CategoryName = @CategoryName WHERE CategoryId = @CategoryId";
//            await _db.ExecuteAsync(sql, c);
//        }

//        public async Task DeleteAsync(int id)
//        {
//            var sql = "DELETE FROM Categories WHERE CategoryId = @Id";
//            await _db.ExecuteAsync(sql, new { Id = id });
//        }
//    }
//}


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
//using System.Collections.Generic;
//using System.Threading.Tasks;
//using Microsoft.EntityFrameworkCore;
//using ProductApp.Data;
//using ProductApp.Models;
//using Dapper;
//using System.Data;

//namespace ProductApp.Repositories
//{
//    public class ProductRepository
//    {

//        private readonly IDbConnection _db;
//        public ProductRepository(IDbConnection db) => _db = db;

//        public async Task<IEnumerable<Product>> GetAllAsync()
//        {
//            var sql = @"SELECT p.*, c.* FROM Products p
//                        INNER JOIN Categories c ON p.CategoryId = c.CategoryId";
//            var productDict = new Dictionary<int, Product>();
//            var result = await _db.QueryAsync<Product, Category, Product>(
//                sql,
//                (product, category) => {
//                    product.Category = category;
//                    return product;
//                },
//                splitOn: "CategoryId"
//            );
//            return result;
//        }

//        public async Task<Product> GetByIdAsync(int id)
//        {
//            var sql = @"SELECT p.*, c.* FROM Products p
//                        INNER JOIN Categories c ON p.CategoryId = c.CategoryId
//                        WHERE p.ProductId = @Id";
//            var productDict = new Dictionary<int, Product>();
//            var result = await _db.QueryAsync<Product, Category, Product>(
//                sql,
//                (product, category) => {
//                    product.Category = category;
//                    return product;
//                },
//                new { Id = id },
//                splitOn: "CategoryId"
//            );
//            return result.FirstOrDefault();
//        }

//        public async Task AddAsync(Product p)
//        {
//            var sql = @"INSERT INTO Products (ProductName, ProductDescription, ProductPrice, ProductStock, CategoryId, ProductImage)
//            VALUES (@ProductName, @ProductDescription, @ProductPrice, @ProductStock, @CategoryId, @ProductImage)";
//            await _db.ExecuteAsync(sql, p);
//        }

//        public async Task UpdateAsync(Product p)
//        {
//            var sql = @"UPDATE Products SET ProductName = @ProductName,ProductDescription = @ProductDescription, ProductPrice = @ProductPrice,ProductStock = @ProductStock, CategoryId = @CategoryId, ProductImage = @ProductImage 
//                        WHERE ProductId = @ProductId";
//            await _db.ExecuteAsync(sql, p);
//        }

//        public async Task DeleteAsync(int id)
//        {
//            var sql = @"DELETE FROM Products WHERE ProductId = @Id";
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
    }
}
using Dapper;
using Dapper.Contrib.Extensions;
using MySqlConnector;
using ProductApp.Models;

namespace ProductApp.Repositories
{
    public class ProductRepository
    {
        private readonly string _connectionString = "Server=localhost;Database=productapp;User=kirito;Password=admin;";

        public IEnumerable<Product> GetAllProducts()
        {
            using var connection = new MySqlConnection(_connectionString);
            return connection.GetAll<Product>();
        }

        public IEnumerable<Product> GetAllProductsWithCategories()
        {
            using var connection = new MySqlConnection(_connectionString);
            var sql = @"
                SELECT p.ProductId, p.ProductName, p.CategoryId, p.ProductPrice, 
                       p.ProductDescription, p.ProductImage, p.ProductStock,
                       c.CategoryId as CategoryId2, c.CategoryName 
                FROM product p 
                LEFT JOIN category c ON p.CategoryId = c.CategoryId";

            return connection.Query<Product, Category, Product>(sql,
                (product, category) =>
                {
                    product.Category = category;
                    return product;
                },
                splitOn: "CategoryId2");
        }

        public Product GetProductById(int id)
        {
            using var connection = new MySqlConnection(_connectionString);
            return connection.Get<Product>(id);
        }

        public Product GetProductByIdWithCategory(int id)
        {
            using var connection = new MySqlConnection(_connectionString);
            var sql = @"
                SELECT p.ProductId, p.ProductName, p.CategoryId, p.ProductPrice, 
                       p.ProductDescription, p.ProductImage, p.ProductStock,
                       c.CategoryId as CategoryId2, c.CategoryName 
                FROM product p 
                LEFT JOIN category c ON p.CategoryId = c.CategoryId 
                WHERE p.ProductId = @ProductId";

            return connection.Query<Product, Category, Product>(sql,
                (product, category) =>
                {
                    product.Category = category;
                    return product;
                },
                new { ProductId = id },
                splitOn: "CategoryId2").FirstOrDefault();
        }

        public long AddProduct(Product product)
        {
            using var connection = new MySqlConnection(_connectionString);
            try
            {
                product.Category = null;
                product.ProductId = 0;
                var sql = @"INSERT INTO product (ProductName, CategoryId, ProductPrice, ProductDescription, ProductImage, ProductStock) 
                           VALUES (@ProductName, @CategoryId, @ProductPrice, @ProductDescription, @ProductImage, @ProductStock);
                           SELECT LAST_INSERT_ID();";

                return connection.QuerySingle<long>(sql, product);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in AddProduct: {ex.Message}");
                throw;
            }
        }

        public bool DeleteProduct(int id)
        {
            using var connection = new MySqlConnection(_connectionString);
            try
            {
                // Use explicit SQL for delete
                var sql = "DELETE FROM product WHERE ProductId = @ProductId";
                var rowsAffected = connection.Execute(sql, new { ProductId = id });
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in DeleteProduct: {ex.Message}");
                throw;
            }
        }

        public bool UpdateProduct(Product product)
        {
            using var connection = new MySqlConnection(_connectionString);
            try
            {
                product.Category = null;

                var sql = @"UPDATE product 
                           SET ProductName = @ProductName, 
                               CategoryId = @CategoryId, 
                               ProductPrice = @ProductPrice, 
                               ProductDescription = @ProductDescription, 
                               ProductImage = @ProductImage, 
                               ProductStock = @ProductStock 
                           WHERE ProductId = @ProductId";

                var rowsAffected = connection.Execute(sql, product);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UpdateProduct: {ex.Message}");
                throw;
            }
        }

        public IEnumerable<Product> GetProductsByCategoryId(int categoryId)
        {
            using var connection = new MySqlConnection(_connectionString);
            return connection.Query<Product>("SELECT * FROM product WHERE CategoryId = @CategoryId", new { CategoryId = categoryId });
        }
    }
}
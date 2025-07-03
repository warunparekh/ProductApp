using Dapper;
using Dapper.Contrib.Extensions;
using MySqlConnector;
using ProductApp.Models;

namespace ProductApp.Repositories
{
    public class CategoryRepository
    {
        private readonly string _connectionString = "Server=localhost;Database=productapp;User=kirito;Password=admin;";

        public IEnumerable<Category> GetAllCategories()
        {
            using var connection = new MySqlConnection(_connectionString);
            return connection.GetAll<Category>();
        }

        public Category GetCategoryById(int id)
        {
            using var connection = new MySqlConnection(_connectionString);
            return connection.Get<Category>(id);
        }

        public long AddCategory(Category category)
        {
            using var connection = new MySqlConnection(_connectionString);
            try
            {
                category.CategoryId = 0; 
                var sql = @"INSERT INTO category (CategoryName) 
                           VALUES (@CategoryName);
                           SELECT LAST_INSERT_ID();";

                return connection.QuerySingle<long>(sql, category);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in AddCategory: {ex.Message}");
                throw;
            }
        }

        public bool DeleteCategory(int id)
        {
            using var connection = new MySqlConnection(_connectionString);
            try
            {
                var productCount = connection.QuerySingle<int>(
                    "SELECT COUNT(*) FROM product WHERE CategoryId = @CategoryId",
                    new { CategoryId = id });

                if (productCount > 0)
                {
                    throw new InvalidOperationException("Cannot delete category that has products assigned to it.");
                }

                var sql = "DELETE FROM category WHERE CategoryId = @CategoryId";
                var rowsAffected = connection.Execute(sql, new { CategoryId = id });
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in DeleteCategory: {ex.Message}");
                throw;
            }
        }

        public bool UpdateCategory(Category category)
        {
            using var connection = new MySqlConnection(_connectionString);
            try
            {
                var sql = @"UPDATE category 
                           SET CategoryName = @CategoryName 
                           WHERE CategoryId = @CategoryId";

                var rowsAffected = connection.Execute(sql, category);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UpdateCategory: {ex.Message}");
                throw;
            }
        }

        public int GetProductCountByCategory(int categoryId)
        {
            using var connection = new MySqlConnection(_connectionString);
            return connection.QuerySingle<int>(
                "SELECT COUNT(*) FROM product WHERE CategoryId = @CategoryId",
                new { CategoryId = categoryId });
        }
    }
}
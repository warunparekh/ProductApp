using Dapper;
using Dapper.Contrib.Extensions;
using MySqlConnector;
using ProductApp.Models;

namespace ProductApp.Repositories
{
    public class CartRepository
    {
        private readonly string _connectionString = "Server=localhost;Database=productapp;User=kirito;Password=admin;";

        public IEnumerable<Cart> GetCartByUser(string userId)
        {
            using var connection = new MySqlConnection(_connectionString);
            return connection.GetAll<Cart>();
        }

        
    }
}

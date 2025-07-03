using Dapper;
using Dapper.Contrib.Extensions;
using MySqlConnector;
using ProductApp.Models;

namespace ProductApp.Repositories
{
    public class UserRepository
    {
        private readonly string _connectionString = "Server=localhost;Database=productapp;User=kirito;Password=admin;";

        public IEnumerable<User> GetAllUsers()
        {
            using var connection = new MySqlConnection(_connectionString);
            return connection.GetAll<User>();
        }

        public User GetUserById(int id)
        {
            using var connection = new MySqlConnection(_connectionString);
            return connection.Get<User>(id);
        }

        public User GetUserByEmail(string email)
        {
            using var connection = new MySqlConnection(_connectionString);
            return connection.QueryFirstOrDefault<User>("SELECT * FROM User WHERE UserEmail = @Email", new { Email = email });
        }

        public User GetUserByEmailAndPassword(string email, string password)
        {
            using var connection = new MySqlConnection(_connectionString);
            return connection.QueryFirstOrDefault<User>("SELECT * FROM User WHERE UserEmail = @Email AND UserPassword = @Password",
                new { Email = email, Password = password });
        }

        public int AddUser(User user)
        {
            using var connection = new MySqlConnection(_connectionString);
            return (int)connection.Insert(user);
        }

        public bool UpdateUser(User user)
        {
            using var connection = new MySqlConnection(_connectionString);
            return connection.Update(user);
        }

        public bool DeleteUser(int id)
        {
            using var connection = new MySqlConnection(_connectionString);
            var user = new User { UserId = id };
            return connection.Delete(user);
        }

        public bool EmailExists(string email)
        {
            using var connection = new MySqlConnection(_connectionString);
            var count = connection.QueryFirstOrDefault<int>("SELECT COUNT(*) FROM User WHERE UserEmail = @Email", new { Email = email });
            return count > 0;
        }
    }
}
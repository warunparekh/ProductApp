using ProductApp.Models;
using ProductApp.Repositories;
using System.Security.Cryptography;
using System.Text;

namespace ProductApp.Services
{
    public class AuthService
    {
        private readonly UserRepository _userRepository;

        public AuthService(UserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        public User Login(string email, string password)
        {
            var hashedPassword = HashPassword(password);
            return _userRepository.GetUserByEmailAndPassword(email, hashedPassword);
        }

        public bool Register(User user)
        {
            if (_userRepository.EmailExists(user.UserEmail))
            {
                return false;
            }

            user.UserPassword = HashPassword(user.UserPassword);
            user.CreationDate = DateTime.Now;
            user.isAdmin = false; 

            return _userRepository.AddUser(user) > 0;
        }

        public bool IsAdmin(User user)
        {
            return user?.isAdmin == true;
        }
    }
}
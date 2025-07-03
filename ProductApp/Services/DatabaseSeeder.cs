using ProductApp.Models;
using ProductApp.Repositories;

namespace ProductApp.Services
{
    public class DatabaseSeeder
    {
        private readonly UserRepository _userRepository;
        private readonly CategoryRepository _categoryRepository;
        private readonly ProductRepository _productRepository;
        private readonly AuthService _authService;

        public DatabaseSeeder(
            UserRepository userRepository,
            CategoryRepository categoryRepository,
            ProductRepository productRepository,
            AuthService authService)
        {
            _userRepository = userRepository;
            _categoryRepository = categoryRepository;
            _productRepository = productRepository;
            _authService = authService;
        }

        public async Task SeedAsync()
        {
            var adminEmail = "admin@productapp.com";
            if (!_userRepository.EmailExists(adminEmail))
            {
                var admin = new User
                {
                    UserName = "Administrator",
                    UserEmail = adminEmail,
                    UserPassword = "Admin@123",
                    UserNumber = 1234567890,
                    UserAddress = "Admin Address",
                    isAdmin = true
                };
                
                admin.UserPassword = _authService.HashPassword(admin.UserPassword);
                admin.CreationDate = DateTime.Now;
                _userRepository.AddUser(admin);
            }            
            
        }
    }
}
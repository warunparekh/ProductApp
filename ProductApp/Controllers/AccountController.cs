using Microsoft.AspNetCore.Mvc;
using ProductApp.Models;
using ProductApp.Services;
using ProductApp.Repositories;

namespace ProductApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly AuthService _authService;
        private readonly UserRepository _userRepository;

        public AccountController()
        {
            _userRepository = new UserRepository();
            _authService = new AuthService(_userRepository);
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Email and password are required.";
                return View();
            }

            var user = _authService.Login(email, password);
            if (user != null)
            {
                
                HttpContext.Session.SetInt32("UserId", user.UserId);
                HttpContext.Session.SetString("UserName", user.UserName);
                HttpContext.Session.SetString("UserEmail", user.UserEmail);
                HttpContext.Session.SetString("IsAdmin", user.isAdmin.ToString());

                if (user.isAdmin)
                {
                    return RedirectToAction("Index", "Admin");
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }

            ViewBag.Error = "Invalid email or password.";
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(User user)
        {
            if (ModelState.IsValid)
            {
                if (_authService.Register(user))
                {
                    TempData["Success"] = "Registration successful! Please login.";
                    return RedirectToAction("Login");
                }
                else
                {
                    ViewBag.Error = "Email already exists.";
                }
            }
            return View(user);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using ProductApp.Models;
using ProductApp.ViewModels;

namespace ProductApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _um;
        private readonly SignInManager<ApplicationUser> _sm;

        public AccountController(
            UserManager<ApplicationUser> um,
            SignInManager<ApplicationUser> sm)
        {
            _um = um;
            _sm = sm;
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel m)
        {
            if (!ModelState.IsValid) return View(m);

            var user = new ApplicationUser
            {
                UserName = m.Email,
                Email = m.Email,
                PhoneNumber = m.PhoneNumber,
                UserAddress = m.UserAddress,
                IsAdmin = false
            };

            var r = await _um.CreateAsync(user, m.Password);
            if (r.Succeeded)
            {
                await _um.AddToRoleAsync(user, "User");
                await _sm.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }
            foreach (var e in r.Errors)
                ModelState.AddModelError("", e.Description);

            return View(m);
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel m, string returnUrl = null)
        {
            if (!ModelState.IsValid) return View(m);

            var r = await _sm.PasswordSignInAsync(m.Email, m.Password, m.RememberMe, false);
            if (r.Succeeded)
            {
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToAction("Index", "Home");
            }
            ModelState.AddModelError("", "Invalid login.");
            return View(m);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _sm.SignOutAsync();
            return RedirectToAction("Login");
        }
    }
}
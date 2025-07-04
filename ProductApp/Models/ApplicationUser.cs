using Microsoft.AspNetCore.Identity;

namespace ProductApp.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string UserAddress { get; set; }
        public bool IsAdmin { get; set; }
    }
}

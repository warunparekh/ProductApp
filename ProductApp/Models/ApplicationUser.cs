using Dapper.Contrib.Extensions;
using Microsoft.AspNetCore.Identity;


namespace ProductApp.Models
{
    [Table("AspNetUsers")]
    public class ApplicationUser : IdentityUser
    {
        public string UserAddress { get; set; }
        public bool IsAdmin { get; set; }
    }
}
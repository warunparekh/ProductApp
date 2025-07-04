using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using Dapper.Contrib.Extensions;

namespace ProductApp.Models
{
    [Dapper.Contrib.Extensions.Table("AspNetUsers")]
    public class ApplicationUser : IdentityUser
    {
        public string UserAddress { get; set; }
        public bool IsAdmin { get; set; }
    }
}
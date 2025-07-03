using Dapper.Contrib.Extensions;
using System.ComponentModel.DataAnnotations;

namespace ProductApp.Models
{
    [Table("User")]
    public class User
    {
        [ExplicitKey]
        public int UserId { get; set; }
        
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string UserName { get; set; }
        
        [Required(ErrorMessage = "Phone number is required")]
        public int UserNumber { get; set; }
        
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string UserEmail { get; set; }
        
        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        public string UserPassword { get; set; }
        
        [Required(ErrorMessage = "Address is required")]
        public string UserAddress { get; set; }
        
        [Computed]
        public DateTime CreationDate { get; set; }
        
        public bool isAdmin { get; set; }
    }
}

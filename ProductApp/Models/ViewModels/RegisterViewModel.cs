using System.ComponentModel.DataAnnotations;

namespace ProductApp.ViewModels
{
    public class RegisterViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, DataType(DataType.Password), MinLength(6)]
        public string Password { get; set; }

        [DataType(DataType.Password), Compare("Password")]
        public string ConfirmPassword { get; set; }

        [Required, Phone]
        public string PhoneNumber { get; set; }

        [Required]
        public string UserAddress { get; set; }
    }
}

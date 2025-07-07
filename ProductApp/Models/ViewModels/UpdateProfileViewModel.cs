using System.ComponentModel.DataAnnotations;

namespace ProductApp.Models.ViewModels
{
    public class UpdateProfileViewModel
    {
        [Required]
        [Display(Name = "Shipping Address")]
        public string UserAddress { get; set; }
    }
}
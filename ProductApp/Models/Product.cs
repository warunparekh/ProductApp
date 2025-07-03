using Dapper.Contrib.Extensions;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ProductApp.Models
{
    [Table("product")]
    public class Product
    {
        [Dapper.Contrib.Extensions.Key]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Product name is required")]
        [StringLength(100, ErrorMessage = "Product name cannot exceed 100 characters")]
        public string ProductName { get; set; }

        [Required(ErrorMessage = "Please select a category")]
        [Display(Name = "Category")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid category")]
        public int CategoryId { get; set; }

        [Computed]
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        [ValidateNever]
        public Category Category { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public int ProductPrice { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string ProductDescription { get; set; }

        public string ProductImage { get; set; }

        [Required(ErrorMessage = "Stock quantity is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Stock cannot be negative")]
        public int ProductStock { get; set; }
    }
}
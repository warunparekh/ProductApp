using Dapper.Contrib.Extensions;
using System.ComponentModel.DataAnnotations;

namespace ProductApp.Models
{
    [Table("category")]
    public class Category
    {
        [Dapper.Contrib.Extensions.Key] 
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Category name is required")]
        [StringLength(100, ErrorMessage = "Category name cannot exceed 100 characters")]
        public string CategoryName { get; set; }
        
        [Computed]
        public int ProductCount { get; set; }
    }
}
using Dapper.Contrib.Extensions;
using System.ComponentModel.DataAnnotations;

namespace ProductApp.Models
{
    [Table("Categories")]
    public class Category
    {
        [Dapper.Contrib.Extensions.Key]
        public int CategoryId { get; set; }

        [Required, StringLength(100)]
        public string CategoryName { get; set; }
    }
}
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductApp.Models
{
    public class Product
    {
        [Key]
        public int ProductId { get; set; }

        [Required, StringLength(200)]
        public string ProductName { get; set; }

        [StringLength(1000)]
        public string ProductDescription { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ProductPrice { get; set; }

        [StringLength(500)]
        public string ProductImage { get; set; }

        public int ProductStock { get; set; }

        public int CategoryId { get; set; }
        public Category Category { get; set; }
    }
}

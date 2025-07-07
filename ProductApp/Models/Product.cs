using Dapper.Contrib.Extensions;

namespace ProductApp.Models
{
    [Table("Products")]
    public class Product
    {
        [Key]
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal ProductPrice { get; set; }
        public string ProductImage { get; set; }
        public int ProductStock { get; set; }
        public string ProductDescription { get; set; }
        public int CategoryId { get; set; }
    }
}
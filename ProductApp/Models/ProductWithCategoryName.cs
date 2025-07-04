namespace ProductApp.Models
{
    public class ProductWithCategoryName
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductDescription { get; set; }
        public decimal ProductPrice { get; set; }
        public int ProductStock { get; set; }
        public string? ProductImage { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
    }
}
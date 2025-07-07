using Dapper.Contrib.Extensions;

namespace ProductApp.Models
{
    [Table("OrderDetails")]
    public class OrderDetails
    {
        [Key]
        public int OrderDetailId { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
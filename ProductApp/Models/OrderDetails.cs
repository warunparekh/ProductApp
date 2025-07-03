using Dapper.Contrib.Extensions;
namespace ProductApp.Models
{
    [Table("OrderDetails")]
    public class OrderDetails
    {
        [ExplicitKey]
        public int OrderDetailsId { get; set; }
        public int OrderId { get; set; }
        public Order Order { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public int ProductQuantity { get; set; }
        public int ProductNetprice { get; set; }
        public int ProductTotalPrice { get; set; }
    }
}

using Dapper.Contrib.Extensions;
namespace ProductApp.Models
{
    [Table("DispatchStatus")]
    public class DispatchStatus
    {
        [ExplicitKey]
        public int DispatchId { get; set; }
        public int OrderId { get; set; }
        public Order Order { get; set; }
        public int OrderDetailsId { get; set; }
        public OrderDetails OrderDetails { get; set; }
        public int OrderStatus { get; set; }
    }
}

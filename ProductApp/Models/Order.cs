using Dapper.Contrib.Extensions;
namespace ProductApp.Models
{
    [Table("Order")]
    public class Order
    {
        [Key]
        public int OrderId { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public int OrderAmount { get; set; }
        public DateTime OrderDate { get; set; }
        public bool isSuccess { get; set; }
    }
}

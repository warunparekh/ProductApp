using Dapper.Contrib.Extensions;
namespace ProductApp.Models
{
    [Table("BillDetails")]
    public class BillDetails
    {
        [ExplicitKey]
        public int BillId { get; set; }
        public int OrderId { get; set; }
        public Order Order { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public int TransactionId { get; set; }
        public Transaction Transaction { get; set; }
        public DateTime BillDate { get; set; }
        public int BillAmount { get; set; }
    }
}

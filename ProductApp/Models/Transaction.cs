using Dapper.Contrib.Extensions;
namespace ProductApp.Models
{
    [Table("Transaction")]
    public class Transaction
    {
        [ExplicitKey]
        public int TransactionId { get; set; }
        public int UserId { get; set; }      
        public User User { get; set; }
        public int TransactionAmount { get; set; }
        public int PaymentMode { get; set; }
        public DateTime TransactionDate { get; set; }
        public bool isSuccess { get; set; }
    }
}

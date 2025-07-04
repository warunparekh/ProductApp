using Dapper.Contrib.Extensions;
namespace ProductApp.Models
{
    [Table("Transaction")]
    public class Transaction
    {
        [ExplicitKey]
        public int TransactionId { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public int TransactionAmount { get; set; }
        public int PaymentMode { get; set; }
        public DateTime TransactionDate { get; set; }
        public bool isSuccess { get; set; }
    }
}

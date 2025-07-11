using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;

namespace ProductApp.Models
{
    [Table("Orders")]
    public class Order
    {
        [Key]
        public int OrderId { get; set; }
        public string UserId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string ShippingAddress { get; set; }
        public DispatchStatus Status { get; set; } 

        [Write(false)]
        public List<OrderDetails> OrderDetails { get; set; } = new List<OrderDetails>();
    }
}
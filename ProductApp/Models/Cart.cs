﻿using Dapper.Contrib.Extensions;
namespace ProductApp.Models
{
    [Table("Cart")]
    public class Cart
    {
        [ExplicitKey]
        public int CartId { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public string ProductName { get; set; }
        public int ProductQuantity { get; set; }
        public int ProductNetprice { get; set; }
        public int CartTotalPrice { get; set; }
    }
}

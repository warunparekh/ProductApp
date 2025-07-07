﻿using Dapper.Contrib.Extensions;

namespace ProductApp.Models
{
    [Table("Cart")]
    public class Cart
    {
        [Key]
        public int CartId { get; set; }
        public string UserId { get; set; }
        public int ProductId { get; set; }
        public int ProductQuantity { get; set; }
    }
}
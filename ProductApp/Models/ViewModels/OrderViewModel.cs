using System.Collections.Generic;

namespace ProductApp.Models.ViewModels
{
    public class OrderViewModel
    {
        public Order Order { get; set; }
        public ApplicationUser User { get; set; }
        public List<OrderDetailViewModel> OrderDetails { get; set; } = new List<OrderDetailViewModel>();
    }

    public class OrderDetailViewModel
    {
        public OrderDetails Detail { get; set; }
        public Product Product { get; set; }
    }
}
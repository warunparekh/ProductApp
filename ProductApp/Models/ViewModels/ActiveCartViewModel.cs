namespace ProductApp.Models.ViewModels
{
    public class ActiveCartViewModel
    {
        public string UserId { get; set; }
        public string UserEmail { get; set; }
        public int ItemCount { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
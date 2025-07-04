namespace ProductApp.ViewModels
{
    public class UserViewModel
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string UserAddress { get; set; }
        public IList<string> Roles { get; set; } = new List<string>();
        public bool IsCurrentUser { get; set; }
    }
}
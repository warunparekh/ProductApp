using System.Collections.Generic;

namespace ProductApp.ViewModels
{
    public class UserViewModel
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string UserAddress { get; set; }
        public ICollection<string> Roles { get; set; }
        public bool IsCurrentUser { get; set; }
    }
}
using System.Collections.Generic;

namespace ProductApp.ViewModels
{
    public class UserViewModel
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string UserAddress { get; set; }
        public IEnumerable<string> Roles { get; set; }
    }
}

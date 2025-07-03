using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ProductApp.Attributes
{
    public class AuthorizeAttribute : ActionFilterAttribute
    {
        public bool AdminRequired { get; set; } = false;

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var userId = context.HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }

            if (AdminRequired)
            {
                var isAdmin = context.HttpContext.Session.GetString("IsAdmin");
                if (isAdmin != "True")
                {
                    context.Result = new RedirectToActionResult("AccessDenied", "Home", null);
                    return;
                }
            }

            base.OnActionExecuting(context);
        }
    }
}
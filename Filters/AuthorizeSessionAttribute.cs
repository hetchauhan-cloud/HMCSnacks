using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HMCSnacks.Filters
{
    public class AuthorizeSession : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var session = context.HttpContext.Session;
            var email = session.GetString("UserEmail");

            if (string.IsNullOrEmpty(email))
            {
                // Redirect to Login if session does not exist
                context.Result = new RedirectToActionResult("Login", "Account", null);
            }

            base.OnActionExecuting(context);
        }
    }
}

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace StargateGalacticCommand.Web.Filters
{
    public sealed class RequireAdminAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.HttpContext.Session.GetString("IsAdmin") != "true")
            {
                context.Result = new RedirectToActionResult("Login", "Admin", null);
            }
        }
    }
}

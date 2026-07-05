using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace StargateGalacticCommand.Web.Filters
{
    public sealed class RequireLoginAttribute : ActionFilterAttribute
    {
        public const string UserIdItemKey = "UserId";

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var userId = context.HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }

            context.HttpContext.Items[UserIdItemKey] = userId.Value;
        }
    }
}

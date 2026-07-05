using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using StargateGalacticCommand.Core.Services;
using StargateGalacticCommand.Data;

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

            var dbContext = context.HttpContext.RequestServices.GetRequiredService<GameDbContext>();
            var activity = context.HttpContext.RequestServices.GetRequiredService<PlayerActivityService>();
            var user = dbContext.Users.Find(userId.Value);
            if (user == null)
            {
                context.HttpContext.Session.Clear();
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }

            activity.MarkSeen(user, DateTime.UtcNow);
            dbContext.SaveChanges();
        }
    }
}

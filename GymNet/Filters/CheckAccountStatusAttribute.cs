using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using GymNet.Models;

namespace GymNet.Filters
{
    public class CheckAccountStatusAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // Skip for anonymous users
            if (!filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                base.OnActionExecuting(filterContext);
                return;
            }

            var userId = filterContext.HttpContext.User.Identity.GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                base.OnActionExecuting(filterContext);
                return;
            }

            var userManager = filterContext.HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            var user = userManager.FindById(userId);

            if (user != null && user.LockoutEndDateUtc.HasValue && user.LockoutEndDateUtc.Value > DateTime.UtcNow)
            {
                // Account is deactivated
                var lockoutEnd = user.LockoutEndDateUtc.Value.ToLocalTime();
                var isAdminDeactivation = (lockoutEnd - DateTime.Now).TotalDays > 365;

                if (isAdminDeactivation)
                {
                    // Sign out the user
                    var authenticationManager = filterContext.HttpContext.GetOwinContext().Authentication;
                    authenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);

                    // Redirect to login with a message
                    filterContext.Result = new RedirectToRouteResult(
                        new RouteValueDictionary
                        {
                            { "controller", "Account" },
                            { "action", "Deactivated" }
                        });
                }
                else
                {
                    // Temporary lockout - sign out and redirect to login
                    var authenticationManager = filterContext.HttpContext.GetOwinContext().Authentication;
                    authenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);

                    filterContext.Result = new RedirectToRouteResult(
                        new RouteValueDictionary
                        {
                            { "controller", "Account" },
                            { "action", "Login" }
                        });
                }
            }

            base.OnActionExecuting(filterContext);
        }
    }
}
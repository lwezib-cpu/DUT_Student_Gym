using System.Security.Claims;
using System.Security.Principal;

namespace GymNet.Helpers
{
    public static class IdentityExtensions
    {
        // Reads the "FirstName" claim added at login (see ApplicationUser.
        // GenerateUserIdentityAsync) so layouts/dashboards can greet someone by
        // name instead of their login email. Falls back to the username (email)
        // if the claim isn't present - e.g. someone still signed in from before
        // this claim was added, who hasn't logged out and back in yet.
        public static string GetFirstName(this IIdentity identity)
        {
            var claimsIdentity = identity as ClaimsIdentity;
            var firstName = claimsIdentity?.FindFirst("FirstName")?.Value;
            return string.IsNullOrWhiteSpace(firstName) ? identity.Name : firstName;
        }
    }
}

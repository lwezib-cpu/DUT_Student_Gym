using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace GymNet.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string Address { get; set; }

        // Staff profile fields - used for trainers (and optionally admin) on the
        // public "Meet the Team" section and pulled through automatically onto
        // any class they create. Uploaded/managed by admin via Manage Trainers.
        public string ProfilePhotoUrl { get; set; }
        public string Specialty { get; set; } // e.g. "Yoga", "Fitness", "Cardio", "Strength Training"

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // authenticationType must match CookieAuthenticationOptions.AuthenticationType in Startup.Auth.cs
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);

            // So layouts/dashboards can greet the person by name instead of their
            // login email, without a database lookup on every page load.
            userIdentity.AddClaim(new Claim("FirstName", FirstName ?? ""));

            return userIdentity;
        }
    }
}
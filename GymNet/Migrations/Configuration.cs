namespace GymNet.Migrations
{
    using GymNet.Data;
    using GymNet.Models;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using System;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        // Runs every time Update-Database executes.
        // Ensures Admin/Member roles exist and seeds one working admin login.
        protected override void Seed(ApplicationDbContext context)
        {
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));

            string[] roles = { "Admin", "Member" };
            foreach (var roleName in roles)
            {
                if (!roleManager.RoleExists(roleName))
                {
                    roleManager.Create(new IdentityRole(roleName));
                }
            }

            const string adminEmail = "admin@gymnet.com";
            if (userManager.FindByEmail(adminEmail) == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    FirstName = "System",
                    LastName = "Administrator",
                    PhoneNumber = "0000000000",
                    DateOfBirth = new DateTime(1990, 1, 1),
                    Gender = "N/A",
                    Address = "GymNet Head Office"
                };

                // Change this password after first login in a real deployment.
                var createResult = userManager.Create(admin, "Admin@123");
                if (createResult.Succeeded)
                {
                    userManager.AddToRole(admin.Id, "Admin");
                }
            }

            // Seed Membership Plans
            if (!context.MembershipPlans.Any())
            {
                context.MembershipPlans.AddRange(new[]
                {
                    new MembershipPlan
                    {
                        Name = "DUT Semester Plan",
                        Description = "Full access to all gym facilities for one semester (6 months). " +
                                     "Includes access to all equipment, locker rooms, and group classes.",
                        Price = 250.00m,
                        DurationInMonths = 6,
                        IsActive = true,
                        CreatedAt = DateTime.Now
                    },
                    new MembershipPlan
                    {
                        Name = "Monthly Plan",
                        Description = "Month-to-month membership with full gym access. " +
                                     "Flexible option for those who don't want long-term commitment.",
                        Price = 80.00m,
                        DurationInMonths = 1,
                        IsActive = true,
                        CreatedAt = DateTime.Now
                    },
                    new MembershipPlan
                    {
                        Name = "Annual Plan",
                        Description = "Best value! Full year of unlimited gym access. " +
                                     "Save 30% compared to monthly plan. Includes 2 free personal training sessions.",
                        Price = 650.00m,
                        DurationInMonths = 12,
                        IsActive = true,
                        CreatedAt = DateTime.Now
                    }
                });

                context.SaveChanges();
            }
        }
    }
}
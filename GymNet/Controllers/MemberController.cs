using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using GymNet.Data;
using GymNet.Models;
using GymNet.ViewModels;

namespace GymNet.Controllers
{
    [Authorize(Roles = "Member")]
    public class MemberController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        // GET: /Member/Dashboard
        public async Task<ActionResult> Dashboard()
        {
            var userId = User.Identity.GetUserId();
            var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId);

            var latestMembership = await db.MemberMemberships
                .Include(m => m.MembershipPlan)
                .Include(m => m.Payments)
                .Where(m => m.UserId == userId)
                .OrderByDescending(m => m.CreatedAt)
                .FirstOrDefaultAsync();

            var viewModel = new MemberDashboardViewModel
            {
                FirstName = user?.FirstName ?? "Member",
                LastName = user?.LastName ?? "",
                Email = user?.Email ?? "",
                PhoneNumber = user?.PhoneNumber ?? "",
                Address = user?.Address ?? "",
                MembershipStatus = latestMembership != null && latestMembership.EndDate > DateTime.Now ? latestMembership.Status : "Inactive",
                MembershipPlan = latestMembership?.MembershipPlan?.Name ?? "None",
                PaymentStatus = latestMembership?.Payments.FirstOrDefault()?.Status ?? "No Payments"
            };

            return View(viewModel);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
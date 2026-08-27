using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using GymNet.Data;
using GymNet.Models;
using GymNet.ViewModels;

namespace GymNet.Controllers
{
    [Authorize(Roles = "Member")]
    public class MemberController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        private ApplicationUserManager UserManager
        {
            get { return HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>(); }
        }

        // GET: /Member/Dashboard
        public async Task<ActionResult> Dashboard()
        {
            var userId = User.Identity.GetUserId();
            var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId);

            // Get the latest membership with plan and payments
            var latestMembership = await db.MemberMemberships
                .Include(m => m.MembershipPlan)
                .Include(m => m.Payments)
                .Where(m => m.UserId == userId)
                .OrderByDescending(m => m.CreatedAt)
                .FirstOrDefaultAsync();

            // Get payment summary
            var totalPayments = await db.Payments
                .Where(p => p.MemberMembership.UserId == userId && p.Status == "Completed")
                .SumAsync(p => (decimal?)p.Amount) ?? 0;

            var lastPayment = await db.Payments
                .Where(p => p.MemberMembership.UserId == userId && p.Status == "Completed")
                .OrderByDescending(p => p.PaymentDate)
                .FirstOrDefaultAsync();

            // Determine payment status
            string paymentStatus;
            if (latestMembership != null && latestMembership.Payments.Any(p => p.Status == "Completed"))
            {
                paymentStatus = "Paid";
            }
            else if (latestMembership != null && latestMembership.Payments.Any(p => p.Status == "Pending"))
            {
                paymentStatus = "Pending";
            }
            else if (latestMembership != null && latestMembership.Payments.Any(p => p.Status == "Failed"))
            {
                paymentStatus = "Failed";
            }
            else
            {
                paymentStatus = "No Payments";
            }

            // Determine membership status
            string membershipStatus;
            if (latestMembership == null)
            {
                membershipStatus = "No Membership";
            }
            else if (latestMembership.Status == "Active" && latestMembership.EndDate > DateTime.Now)
            {
                membershipStatus = "Active";
            }
            else if (latestMembership.Status == "PendingPayment")
            {
                membershipStatus = "Pending Payment";
            }
            else if (latestMembership.EndDate <= DateTime.Now)
            {
                membershipStatus = "Expired";
            }
            else
            {
                membershipStatus = latestMembership.Status;
            }

            var model = new MemberDashboardViewModel
            {
                FirstName = user?.FirstName ?? "Member",
                LastName = user?.LastName ?? "",
                Email = user?.Email ?? "",
                PhoneNumber = user?.PhoneNumber ?? "",
                Address = user?.Address ?? "",
                Gender = user?.Gender ?? "",
                DateOfBirth = user?.DateOfBirth ?? DateTime.Now,

                // Membership info
                MembershipStatus = membershipStatus,
                MembershipPlan = latestMembership?.MembershipPlan?.Name ?? "No Plan",
                MembershipEndDate = latestMembership?.EndDate,
                DaysRemaining = latestMembership != null ? (latestMembership.EndDate - DateTime.Now).Days : 0,
                HasActiveMembership = latestMembership != null &&
                                     latestMembership.Status == "Active" &&
                                     latestMembership.EndDate > DateTime.Now,
                MemberSince = latestMembership?.StartDate,

                // Payment info
                PaymentStatus = paymentStatus,
                TotalPayments = totalPayments,
                LastPaymentDate = lastPayment?.PaymentDate,
                LastPaymentAmount = lastPayment?.Amount ?? 0
            };

            return View(model);
        }
        // GET: /Membership/MyMembership
        public async Task<ActionResult> MyMembership()
        {
            var userId = User.Identity.GetUserId();

            var membership = await db.MemberMemberships
                .Include(m => m.MembershipPlan)
                .Include(m => m.Payments)
                .Where(m => m.UserId == userId)
                .OrderByDescending(m => m.CreatedAt)
                .FirstOrDefaultAsync();

            if (membership == null)
            {
                return RedirectToAction("Plans");
            }

            // Get latest completed payment
            var latestPayment = membership.Payments
                .Where(p => p.Status == "Completed")
                .OrderByDescending(p => p.PaymentDate)
                .FirstOrDefault();

            // Get all payments for this membership
            var allPayments = membership.Payments
                .OrderByDescending(p => p.PaymentDate)
                .ToList();

            var model = new MemberMembershipViewModel
            {
                Id = membership.Id,
                PlanName = membership.MembershipPlan.Name,
                PlanDescription = membership.MembershipPlan.Description,
                Price = membership.MembershipPlan.Price,
                DurationInMonths = membership.MembershipPlan.DurationInMonths,
                StartDate = membership.StartDate,
                EndDate = membership.EndDate,
                Status = membership.Status,
                DaysRemaining = (membership.EndDate - DateTime.Now).Days,
                PaymentStatus = latestPayment?.Status ?? "No Payment",
                LastPaymentDate = latestPayment?.PaymentDate,
                LastPaymentAmount = latestPayment?.Amount ?? 0,
                TransactionReference = latestPayment?.TransactionReference ?? "N/A",
                PaymentMethod = latestPayment?.PaymentMethod ?? "N/A",
                TotalPaid = allPayments.Where(p => p.Status == "Completed").Sum(p => p.Amount)
            };

            return View(model);
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
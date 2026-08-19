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
    public class MembershipController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        private ApplicationUserManager UserManager
        {
            get { return HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>(); }
        }

        // GET: /Membership/Plans
        [AllowAnonymous]
        public ActionResult Plans()
        {
            var plans = db.MembershipPlans
                .Where(p => p.IsActive)
                .OrderBy(p => p.Price)
                .Select(p => new MembershipPlanViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    DurationInMonths = p.DurationInMonths,
                    IsActive = p.IsActive
                })
                .ToList();

            return View(plans);
        }

        // GET: /Membership/SelectPlan/{id}
        public async Task<ActionResult> SelectPlan(int id)
        {
            var plan = await db.MembershipPlans.FindAsync(id);
            if (plan == null || !plan.IsActive)
            {
                TempData["ErrorMessage"] = "Selected membership plan is not available.";
                return RedirectToAction("Plans");
            }

            // Check if user already has an active membership
            var userId = User.Identity.GetUserId();
            var hasActiveMembership = await db.MemberMemberships
                .AnyAsync(m => m.UserId == userId && m.Status == "Active" && m.EndDate > DateTime.Now);

            if (hasActiveMembership)
            {
                TempData["ErrorMessage"] = "You already have an active membership.";
                return RedirectToAction("MyMembership");
            }

            var model = new SelectMembershipViewModel
            {
                PlanId = plan.Id,
                PlanName = plan.Name,
                Price = plan.Price,
                DurationInMonths = plan.DurationInMonths
            };

            return View(model);
        }

        // GET: /Membership/Payment/{planId}
        public async Task<ActionResult> Payment(int planId)
        {
            var plan = await db.MembershipPlans.FindAsync(planId);
            if (plan == null || !plan.IsActive)
            {
                TempData["ErrorMessage"] = "Invalid membership plan.";
                return RedirectToAction("Plans");
            }

            var model = new PaymentViewModel
            {
                MembershipPlanId = plan.Id
            };

            ViewBag.PlanName = plan.Name;
            ViewBag.Price = plan.Price;
            ViewBag.DurationInMonths = plan.DurationInMonths;

            return View(model);
        }

        // POST: /Membership/Payment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Payment(PaymentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var plan = await db.MembershipPlans.FindAsync(model.MembershipPlanId);
                ViewBag.PlanName = plan?.Name;
                ViewBag.Price = plan?.Price;
                ViewBag.DurationInMonths = plan?.DurationInMonths;
                return View(model);
            }

            var planInfo = await db.MembershipPlans.FindAsync(model.MembershipPlanId);
            if (planInfo == null)
            {
                return HttpNotFound();
            }

            // Generate a demo transaction reference
            var transactionRef = "GYM-" + DateTime.Now.ToString("yyyyMMdd") + "-" +
                                Guid.NewGuid().ToString().Substring(0, 8).ToUpper();

            // Create the membership record
            var membership = new MemberMembership
            {
                UserId = User.Identity.GetUserId(),
                MembershipPlanId = model.MembershipPlanId,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddMonths(planInfo.DurationInMonths),
                Status = "Active",
                CreatedAt = DateTime.Now
            };

            db.MemberMemberships.Add(membership);

            // Create payment record
            var payment = new Payment
            {
                MemberMembership = membership,
                Amount = planInfo.Price,
                PaymentMethod = model.PaymentMethod,
                Status = "Completed",
                PaymentDate = DateTime.Now,
                CardHolderName = model.CardHolderName,
                CardNumber = "****" + model.CardNumber.Substring(Math.Max(0, model.CardNumber.Length - 4)),
                ExpiryDate = model.ExpiryDate,
                BankName = model.BankName,
                AccountNumber = model.AccountNumber,
                TransactionReference = transactionRef
            };

            db.Payments.Add(payment);
            await db.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Payment successful! Your {planInfo.Name} is now active. Transaction reference: {transactionRef}";
            return RedirectToAction("PaymentConfirmation", new { membershipId = membership.Id });
        }

        // GET: /Membership/PaymentConfirmation/{membershipId}
        public async Task<ActionResult> PaymentConfirmation(int membershipId)
        {
            // Extract userId into a local string variable first
            var userId = User.Identity.GetUserId();

            var membership = await db.MemberMemberships
                .Include(m => m.MembershipPlan)
                .Include(m => m.Payments)
                .FirstOrDefaultAsync(m => m.Id == membershipId && m.UserId == userId);

            if (membership == null)
            {
                return HttpNotFound();
            }

            return View(membership);
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

            var model = new MemberMembershipViewModel
            {
                Id = membership.Id,
                PlanName = membership.MembershipPlan.Name,
                Price = membership.MembershipPlan.Price,
                StartDate = membership.StartDate,
                EndDate = membership.EndDate,
                Status = membership.Status,
                DaysRemaining = (membership.EndDate - DateTime.Now).Days
            };

            return View(model);
        }

        // GET: /Membership/PaymentHistory
        public async Task<ActionResult> PaymentHistory()
        {
            var userId = User.Identity.GetUserId();

            var payments = await db.Payments
                .Where(p => p.MemberMembership.UserId == userId)
                .OrderByDescending(p => p.PaymentDate)
                .Select(p => new PaymentHistoryViewModel
                {
                    Id = p.Id,
                    Amount = p.Amount,
                    PaymentMethod = p.PaymentMethod,
                    PaymentDate = p.PaymentDate,
                    Status = p.Status,
                    TransactionReference = p.TransactionReference,
                    CardHolderName = p.CardHolderName,
                    MaskedCardNumber = p.CardNumber,
                    BankName = p.BankName,
                    MembershipPlanName = p.MemberMembership.MembershipPlan.Name,
                    MembershipStartDate = p.MemberMembership.StartDate,
                    MembershipEndDate = p.MemberMembership.EndDate
                })
                .ToListAsync();

            return View(payments);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
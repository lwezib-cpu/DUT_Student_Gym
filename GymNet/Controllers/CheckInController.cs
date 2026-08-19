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
    public class CheckInController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        // GET: /CheckIn/Index
        public async Task<ActionResult> Index()
        {
            var userId = User.Identity.GetUserId();

            // Get user's latest membership
            var latestMembership = await db.MemberMemberships
                .Include(m => m.MembershipPlan)
                .Include(m => m.Payments)
                .Where(m => m.UserId == userId)
                .OrderByDescending(m => m.CreatedAt)
                .FirstOrDefaultAsync();

            var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId);

            var model = new CheckInViewModel
            {
                UserId = userId,
                FullName = user?.FirstName + " " + user?.LastName ?? "Member",
                MembershipStatus = latestMembership?.Status ?? "No Membership",
                MembershipPlan = latestMembership?.MembershipPlan?.Name ?? "N/A",
                MembershipEndDate = latestMembership?.EndDate,
                CanCheckIn = false,
                Message = ""
            };

            // Check if user can check in
            if (latestMembership == null)
            {
                model.CanCheckIn = false;
                model.Message = "You don't have a membership. Please purchase a membership plan to check in.";
            }
            else if (latestMembership.Status != "Active")
            {
                model.CanCheckIn = false;
                model.Message = "Your membership is not active. Status: " + latestMembership.Status;
            }
            else if (latestMembership.EndDate < DateTime.Now)
            {
                model.CanCheckIn = false;
                model.Message = "Your membership has expired. Please renew your membership to check in.";
            }
            else if (!latestMembership.Payments.Any(p => p.Status == "Completed"))
            {
                model.CanCheckIn = false;
                model.Message = "No completed payment found. Please complete your payment to check in.";
            }
            else
            {
                model.CanCheckIn = true;
                model.Message = "You're eligible to check in. Scan the QR code at the gym entrance.";
            }

            return View(model);
        }

        // POST: /CheckIn/ProcessQRCode
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ProcessQRCode(string qrCodeContent)
        {
            var userId = User.Identity.GetUserId();

            if (string.IsNullOrWhiteSpace(qrCodeContent))
            {
                return Json(new CheckInResultViewModel
                {
                    Success = false,
                    Message = "Invalid QR code. Please try again."
                });
            }

            // Verify QR code is valid
            if (!qrCodeContent.StartsWith("GYMNET-CHECKIN-"))
            {
                return Json(new CheckInResultViewModel
                {
                    Success = false,
                    Message = "Invalid QR code format. Please scan the correct QR code."
                });
            }

            // Check membership status
            var latestMembership = await db.MemberMemberships
                .Include(m => m.Payments)
                .Where(m => m.UserId == userId)
                .OrderByDescending(m => m.CreatedAt)
                .FirstOrDefaultAsync();

            if (latestMembership == null)
            {
                return Json(new CheckInResultViewModel
                {
                    Success = false,
                    Message = "No membership found. Please purchase a membership plan."
                });
            }

            if (latestMembership.Status != "Active")
            {
                return Json(new CheckInResultViewModel
                {
                    Success = false,
                    Message = "Your membership is not active."
                });
            }

            if (latestMembership.EndDate < DateTime.Now)
            {
                return Json(new CheckInResultViewModel
                {
                    Success = false,
                    Message = "Your membership has expired. Please renew."
                });
            }

            if (!latestMembership.Payments.Any(p => p.Status == "Completed"))
            {
                return Json(new CheckInResultViewModel
                {
                    Success = false,
                    Message = "Payment not completed. Please complete your payment."
                });
            }

            // Check if already checked in today
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            var alreadyCheckedIn = await db.CheckIns
                .AnyAsync(c => c.UserId == userId &&
                               c.CheckInTime >= today &&
                               c.CheckInTime < tomorrow);

            if (alreadyCheckedIn)
            {
                return Json(new CheckInResultViewModel
                {
                    Success = false,
                    Message = "You've already checked in today."
                });
            }

            // Create check-in record
            var checkIn = new CheckIn
            {
                UserId = userId,
                CheckInTime = DateTime.Now,
                CheckInMethod = "QR Code",
                QRCode = qrCodeContent
            };

            db.CheckIns.Add(checkIn);
            await db.SaveChangesAsync();

            return Json(new CheckInResultViewModel
            {
                Success = true,
                Message = "Check-in successful! Welcome to GymNet!",
                CheckInTime = checkIn.CheckInTime
            });
        }

        // GET: /CheckIn/History
        public async Task<ActionResult> History()
        {
            var userId = User.Identity.GetUserId();

            var checkIns = await db.CheckIns
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.CheckInTime)
                .Select(c => new CheckInHistoryViewModel
                {
                    Id = c.Id,
                    FullName = c.User.FirstName + " " + c.User.LastName,
                    Email = c.User.Email,
                    CheckInTime = c.CheckInTime,
                    CheckInMethod = c.CheckInMethod,
                    QRCode = c.QRCode
                })
                .ToListAsync();

            return View(checkIns);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
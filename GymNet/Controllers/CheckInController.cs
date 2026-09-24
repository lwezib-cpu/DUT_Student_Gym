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

            // A visit that has a CheckInTime but no CheckOutTime yet is "open" - the member is in the gym now.
            var openCheckIn = await db.CheckIns
                .Where(c => c.UserId == userId && c.CheckOutTime == null)
                .OrderByDescending(c => c.CheckInTime)
                .FirstOrDefaultAsync();

            var monthStart = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            var totalMinutesThisMonth = await db.CheckIns
                .Where(c => c.UserId == userId && c.CheckOutTime != null && c.CheckInTime >= monthStart)
                .SumAsync(c => (int?)c.DurationMinutes) ?? 0;
            var totalMinutesAllTime = await db.CheckIns
                .Where(c => c.UserId == userId && c.CheckOutTime != null)
                .SumAsync(c => (int?)c.DurationMinutes) ?? 0;

            var model = new CheckInViewModel
            {
                UserId = userId,
                FullName = user?.FirstName + " " + user?.LastName ?? "Member",
                MembershipStatus = latestMembership?.Status ?? "No Membership",
                MembershipPlan = latestMembership?.MembershipPlan?.Name ?? "N/A",
                MembershipEndDate = latestMembership?.EndDate,
                CanCheckIn = false,
                Message = "",
                IsCheckedInNow = openCheckIn != null,
                OpenCheckInId = openCheckIn?.Id,
                OpenCheckInTime = openCheckIn?.CheckInTime,
                TotalMinutesThisMonth = totalMinutesThisMonth,
                TotalMinutesAllTime = totalMinutesAllTime
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
            else if (model.IsCheckedInNow)
            {
                model.CanCheckIn = false; // scanner is for checking in; use the Check Out button below
                model.Message = "You're currently checked in. Scan or tap Check Out when you leave.";
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

            // A member can't check in again while they still have an open (not checked-out) visit
            var openCheckIn = await db.CheckIns
                .AnyAsync(c => c.UserId == userId && c.CheckOutTime == null);

            if (openCheckIn)
            {
                return Json(new CheckInResultViewModel
                {
                    Success = false,
                    Message = "You're already checked in. Please check out first."
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

        // POST: /CheckIn/CheckOut
        // Closes the member's current open visit and records how long they were in the gym.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CheckOut()
        {
            var userId = User.Identity.GetUserId();

            var openCheckIn = await db.CheckIns
                .Where(c => c.UserId == userId && c.CheckOutTime == null)
                .OrderByDescending(c => c.CheckInTime)
                .FirstOrDefaultAsync();

            if (openCheckIn == null)
            {
                return Json(new CheckInResultViewModel
                {
                    Success = false,
                    Message = "You don't have an open check-in to close."
                });
            }

            openCheckIn.CheckOutTime = DateTime.Now;
            openCheckIn.DurationMinutes = (int)Math.Round((openCheckIn.CheckOutTime.Value - openCheckIn.CheckInTime).TotalMinutes);
            if (openCheckIn.DurationMinutes < 0) openCheckIn.DurationMinutes = 0;

            await db.SaveChangesAsync();

            return Json(new CheckInResultViewModel
            {
                Success = true,
                Message = "Checked out! You spent " + FormatDuration(openCheckIn.DurationMinutes.Value) + " at the gym.",
                CheckInTime = openCheckIn.CheckOutTime
            });
        }

        private static string FormatDuration(int minutes)
        {
            var hours = minutes / 60;
            var mins = minutes % 60;
            return hours > 0 ? $"{hours}h {mins}m" : $"{mins}m";
        }

        // GET: /CheckIn/History
        // GET: /CheckIn/History
        public async Task<ActionResult> History(int? month, int? year)
        {
            var userId = User.Identity.GetUserId();

            var checkIns = await db.CheckIns
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.CheckInTime)
                .Select(c => new CheckInHistoryViewModel
                {
                    Id = c.Id,
                    UserId = c.UserId,
                    FullName = c.User.FirstName + " " + c.User.LastName,
                    Email = c.User.Email,
                    CheckInTime = c.CheckInTime,
                    CheckInMethod = c.CheckInMethod,
                    QRCode = c.QRCode,
                    CheckOutTime = c.CheckOutTime,
                    DurationMinutes = c.DurationMinutes
                })
                .ToListAsync();

            var calendarMonth = month ?? DateTime.Today.Month;
            var calendarYear = year ?? DateTime.Today.Year;

            // Clamp to a valid month/year rather than erroring on a bad query string
            if (calendarMonth < 1) calendarMonth = 1;
            if (calendarMonth > 12) calendarMonth = 12;

            var checkedInDays = checkIns
                .Where(c => c.CheckInTime.Month == calendarMonth && c.CheckInTime.Year == calendarYear)
                .Select(c => c.CheckInTime.Day)
                .Distinct()
                .ToList();

            ViewBag.CalendarMonth = calendarMonth;
            ViewBag.CalendarYear = calendarYear;
            ViewBag.CheckedInDays = checkedInDays;

            return View(checkIns);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
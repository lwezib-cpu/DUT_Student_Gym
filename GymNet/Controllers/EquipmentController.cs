using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using GymNet.Data;
using GymNet.Helpers;
using GymNet.Models;
using GymNet.ViewModels;

namespace GymNet.Controllers
{
    [Authorize(Roles = "Member")]
    public class EquipmentController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        // GET: /Equipment
        public async Task<ActionResult> Index()
        {
            var equipment = await db.Equipment
                .Where(e => e.IsActive)
                .Include(e => e.Bookings)
                .OrderBy(e => e.Name)
                .ToListAsync();

            var model = equipment.Select(e => new EquipmentListItemViewModel
            {
                Id = e.Id,
                Name = e.Name,
                Description = e.Description,
                TotalQuantity = e.TotalQuantity,
                AvailableNow = e.IsUnderMaintenance ? 0 : e.TotalQuantity - e.Bookings.Count(b => b.Status == "PendingApproval" || b.Status == "Reserved"),
                ReservationFee = e.ReservationFee,
                OveragePerHourFee = e.OveragePerHourFee,
                IconClass = EquipmentVisuals.IconFor(e.Name),
                AnimationClass = EquipmentVisuals.AnimationFor(e.Name),
                LottieUrl = EquipmentVisuals.LottieFor(e.Name),
                ImageUrl = EquipmentVisuals.ImageFor(e.Name),
                IsUnderMaintenance = e.IsUnderMaintenance,
                MaintenanceNote = e.MaintenanceNote
            }).ToList();

            return View(model);
        }

        // POST: /Equipment/CancelBooking/{id} - a member can withdraw their own
        // request only while it's still awaiting admin approval.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CancelBooking(int id)
        {
            var userId = User.Identity.GetUserId();

            var booking = await db.EquipmentBookings
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId && b.Status == "PendingApproval");

            if (booking == null)
            {
                TempData["ErrorMessage"] = "That booking can't be cancelled - it may already be confirmed or actioned.";
                return RedirectToAction("MyBookings");
            }

            booking.Status = "Cancelled";
            await db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Booking request cancelled.";
            return RedirectToAction("MyBookings");
        }

        // POST: /Equipment/Reserve
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Reserve(ReserveEquipmentViewModel model)
        {
            var equipment = await db.Equipment
                .Include(e => e.Bookings)
                .FirstOrDefaultAsync(e => e.Id == model.EquipmentId && e.IsActive);

            if (equipment == null)
            {
                TempData["ErrorMessage"] = "That equipment isn't available.";
                return RedirectToAction("Index");
            }

            if (equipment.IsUnderMaintenance)
            {
                TempData["ErrorMessage"] = equipment.Name + " is currently under maintenance and can't be booked right now.";
                return RedirectToAction("Index");
            }

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please choose a valid duration (15-240 minutes).";
                return RedirectToAction("Index");
            }

            var reservedCount = equipment.Bookings.Count(b => b.Status == "PendingApproval" || b.Status == "Reserved");
            if (reservedCount >= equipment.TotalQuantity)
            {
                TempData["ErrorMessage"] = "All " + equipment.Name + " units are currently reserved.";
                return RedirectToAction("Index");
            }

            var userId = User.Identity.GetUserId();
            var now = DateTime.Now;

            db.EquipmentBookings.Add(new EquipmentBooking
            {
                EquipmentId = equipment.Id,
                UserId = userId,
                ReservedAt = now,
                ExpectedDurationMinutes = model.ExpectedDurationMinutes,
                ExpectedReturnAt = now.AddMinutes(model.ExpectedDurationMinutes),
                ReservationFeeCharged = equipment.ReservationFee,
                Status = "PendingApproval"
            });

            await db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Request sent! " + equipment.Name + " for " + model.ExpectedDurationMinutes +
                " minutes is awaiting admin approval before it's confirmed.";
            return RedirectToAction("MyBookings");
        }

        // GET: /Equipment/MyBookings
        public async Task<ActionResult> MyBookings()
        {
            var userId = User.Identity.GetUserId();

            var bookings = await db.EquipmentBookings
                .Include(b => b.Equipment)
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.ReservedAt)
                .Select(b => new EquipmentBookingListItemViewModel
                {
                    Id = b.Id,
                    EquipmentName = b.Equipment.Name,
                    ReservedAt = b.ReservedAt,
                    ExpectedReturnAt = b.ExpectedReturnAt,
                    ActualReturnAt = b.ActualReturnAt,
                    ReservationFeeCharged = b.ReservationFeeCharged,
                    OverageFeeCharged = b.OverageFeeCharged,
                    Status = b.Status,
                    DeclineReason = b.DeclineReason
                })
                .ToListAsync();

            return View(bookings);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}

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
    public class ClassController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        // GET: /Class - upcoming classes a member can join
        public async Task<ActionResult> Index()
        {
            var userId = User.Identity.GetUserId();
            var now = DateTime.Now;

            var classes = await db.GymClasses
                .Include(c => c.Trainer)
                .Include(c => c.Bookings)
                .Where(c => c.Status == "Scheduled" && c.StartTime >= now)
                .OrderBy(c => c.StartTime)
                .ToListAsync();

            var model = classes.Select(c => new ClassListItemViewModel
            {
                Id = c.Id,
                Title = c.Title,
                Description = c.Description,
                TrainerName = c.Trainer.FirstName + " " + c.Trainer.LastName,
                TrainerPhotoUrl = c.Trainer.ProfilePhotoUrl,
                TrainerSpecialty = c.Trainer.Specialty,
                StartTime = c.StartTime,
                DurationMinutes = c.DurationMinutes,
                Capacity = c.Capacity,
                BookedCount = c.Bookings.Count(b => b.Status == "Booked"),
                Status = c.Status,
                IsBookedByCurrentUser = c.Bookings.Any(b => b.UserId == userId && b.Status == "Booked"),
                IsWaitlistedByCurrentUser = c.Bookings.Any(b => b.UserId == userId && b.Status == "Waitlisted"),
                WaitlistCount = c.Bookings.Count(b => b.Status == "Waitlisted")
            }).ToList();

            return View(model);
        }

        // POST: /Class/Join/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Join(int id)
        {
            var userId = User.Identity.GetUserId();

            var gymClass = await db.GymClasses
                .Include(c => c.Bookings)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (gymClass == null || gymClass.Status != "Scheduled" || gymClass.StartTime <= DateTime.Now)
            {
                TempData["ErrorMessage"] = "This class is no longer available.";
                return RedirectToAction("Index");
            }

            var alreadyIn = gymClass.Bookings.Any(b => b.UserId == userId && (b.Status == "Booked" || b.Status == "Waitlisted"));
            if (alreadyIn)
            {
                TempData["ErrorMessage"] = "You're already in this class or on its waitlist.";
                return RedirectToAction("Index");
            }

            var bookedCount = gymClass.Bookings.Count(b => b.Status == "Booked");
            var isFull = bookedCount >= gymClass.Capacity;

            db.ClassBookings.Add(new ClassBooking
            {
                GymClassId = id,
                UserId = userId,
                BookedAt = DateTime.Now,
                Status = isFull ? "Waitlisted" : "Booked"
            });

            await db.SaveChangesAsync();

            TempData["SuccessMessage"] = isFull
                ? "The class is full - you've been added to the waitlist and we'll move you in if a spot opens up."
                : "You're in! See you at \"" + gymClass.Title + "\".";
            return RedirectToAction("Index");
        }

        // POST: /Class/Leave/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Leave(int id)
        {
            var userId = User.Identity.GetUserId();

            var booking = await db.ClassBookings
                .FirstOrDefaultAsync(b => b.GymClassId == id && b.UserId == userId && (b.Status == "Booked" || b.Status == "Waitlisted"));

            if (booking == null)
            {
                TempData["ErrorMessage"] = "You're not booked into that class.";
                return RedirectToAction("Index");
            }

            var wasBooked = booking.Status == "Booked";
            booking.Status = "Cancelled";
            await db.SaveChangesAsync();

            // If a confirmed spot just opened up, promote the longest-waiting person on the waitlist.
            if (wasBooked)
            {
                var nextInLine = await db.ClassBookings
                    .Where(b => b.GymClassId == id && b.Status == "Waitlisted")
                    .OrderBy(b => b.BookedAt)
                    .FirstOrDefaultAsync();

                if (nextInLine != null)
                {
                    nextInLine.Status = "Booked";
                    await db.SaveChangesAsync();
                }
            }

            TempData["SuccessMessage"] = "You've left the class.";
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}

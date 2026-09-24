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
    [Authorize(Roles = "Trainer")]
    public class TrainerController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        // GET: /Trainer/Dashboard
        public async Task<ActionResult> Dashboard()
        {
            var trainerId = User.Identity.GetUserId();
            var now = DateTime.Now;

            var classes = await db.GymClasses
                .Include(c => c.Bookings)
                .Where(c => c.TrainerId == trainerId && c.Status != "Cancelled")
                .OrderBy(c => c.StartTime)
                .ToListAsync();

            var model = new TrainerDashboardViewModel
            {
                UpcomingClasses = classes.Where(c => c.StartTime >= now).Select(ToListItem).ToList(),
                PastClasses = classes.Where(c => c.StartTime < now).OrderByDescending(c => c.StartTime).Take(10).Select(ToListItem).ToList(),
                TotalMembersCoached = classes.SelectMany(c => c.Bookings).Where(b => b.Status == "Booked").Select(b => b.UserId).Distinct().Count(),
                TotalFeedbackGiven = await db.TrainerFeedbacks.CountAsync(f => f.TrainerId == trainerId)
            };

            return View(model);
        }

        private static ClassListItemViewModel ToListItem(GymClass c)
        {
            return new ClassListItemViewModel
            {
                Id = c.Id,
                Title = c.Title,
                Description = c.Description,
                StartTime = c.StartTime,
                DurationMinutes = c.DurationMinutes,
                Capacity = c.Capacity,
                BookedCount = c.Bookings?.Count(b => b.Status == "Booked") ?? 0,
                Status = c.Status
            };
        }

        // GET: /Trainer/CreateClass
        public ActionResult CreateClass()
        {
            return View(new CreateClassViewModel());
        }

        // POST: /Trainer/CreateClass
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateClass(CreateClassViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (model.StartTime <= DateTime.Now)
            {
                ModelState.AddModelError(nameof(model.StartTime), "Start time must be in the future.");
                return View(model);
            }

            var gymClass = new GymClass
            {
                TrainerId = User.Identity.GetUserId(),
                Title = model.Title,
                Description = model.Description,
                StartTime = model.StartTime,
                DurationMinutes = model.DurationMinutes,
                Capacity = model.Capacity,
                Status = "Scheduled",
                CreatedAt = DateTime.Now
            };

            db.GymClasses.Add(gymClass);
            await db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Class \"" + gymClass.Title + "\" created. Members will see it on the Classes page.";
            return RedirectToAction("Dashboard");
        }

        // POST: /Trainer/CancelClass/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CancelClass(int id)
        {
            var trainerId = User.Identity.GetUserId();
            var gymClass = await db.GymClasses.FirstOrDefaultAsync(c => c.Id == id && c.TrainerId == trainerId);
            if (gymClass == null)
            {
                return HttpNotFound();
            }

            gymClass.Status = "Cancelled";
            await db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Class cancelled.";
            return RedirectToAction("Dashboard");
        }

        // GET: /Trainer/Members - members this trainer has coached (booked into at least one of their classes)
        public async Task<ActionResult> Members()
        {
            var trainerId = User.Identity.GetUserId();

            var members = await db.ClassBookings
                .Where(b => b.GymClass.TrainerId == trainerId && b.Status == "Booked")
                .Select(b => b.User)
                .Distinct()
                .Select(u => new TrainerMemberListItemViewModel
                {
                    UserId = u.Id,
                    FullName = u.FirstName + " " + u.LastName,
                    Email = u.Email
                })
                .ToListAsync();

            return View(members);
        }

        // GET: /Trainer/GiveFeedback/{memberId}
        public async Task<ActionResult> GiveFeedback(string memberId)
        {
            var member = await db.Users.FirstOrDefaultAsync(u => u.Id == memberId);
            if (member == null)
            {
                return HttpNotFound();
            }

            var model = new GiveFeedbackViewModel
            {
                MemberId = member.Id,
                MemberName = member.FirstName + " " + member.LastName
            };

            return View(model);
        }

        // POST: /Trainer/GiveFeedback
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> GiveFeedback(GiveFeedbackViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var member = await db.Users.FirstOrDefaultAsync(u => u.Id == model.MemberId);
                model.MemberName = member?.FirstName + " " + member?.LastName;
                return View(model);
            }

            var feedback = new TrainerFeedback
            {
                TrainerId = User.Identity.GetUserId(),
                MemberId = model.MemberId,
                Comment = model.Comment,
                Rating = model.Rating,
                CreatedAt = DateTime.Now
            };

            db.TrainerFeedbacks.Add(feedback);
            await db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Feedback sent to " + model.MemberName + ".";
            return RedirectToAction("Members");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}

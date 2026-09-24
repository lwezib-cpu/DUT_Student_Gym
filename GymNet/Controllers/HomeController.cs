using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using GymNet.Data;
using GymNet.ViewModels;

namespace GymNet.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        public async Task<ActionResult> Index()
        {
            var trainerRoleId = await db.Roles.Where(r => r.Name == "Trainer").Select(r => r.Id).FirstOrDefaultAsync();

            var staff = await db.Users
                .Where(u => u.Roles.Any(r => r.RoleId == trainerRoleId))
                .Select(u => new TrainerListItemViewModel
                {
                    UserId = u.Id,
                    FullName = u.FirstName + " " + u.LastName,
                    Specialty = u.Specialty,
                    ProfilePhotoUrl = u.ProfilePhotoUrl
                })
                .ToListAsync();

            ViewBag.Staff = staff;

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
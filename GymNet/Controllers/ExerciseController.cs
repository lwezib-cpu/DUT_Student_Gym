using System.Web.Mvc;
using GymNet.Helpers;
using GymNet.ViewModels;

namespace GymNet.Controllers
{
    [Authorize(Roles = "Member")]
    public class ExerciseController : Controller
    {
        // GET: /Exercise
        // GET: /Exercise?bodyPart=Chest
        public ActionResult Index(string bodyPart)
        {
            var model = new ExerciseRecommendationViewModel
            {
                BodyParts = ExerciseCatalog.BodyParts,
                SelectedBodyPart = bodyPart,
                Exercises = ExerciseCatalog.ForBodyPart(bodyPart)
            };

            return View(model);
        }
    }
}

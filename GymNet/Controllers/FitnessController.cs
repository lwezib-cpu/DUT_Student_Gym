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
    public class FitnessController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        // GET: /Fitness/LogWorkout
        public ActionResult LogWorkout()
        {
            return View(new LogWorkoutViewModel());
        }

        // POST: /Fitness/LogWorkout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> LogWorkout(LogWorkoutViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = User.Identity.GetUserId();

            db.WorkoutLogs.Add(new WorkoutLog
            {
                UserId = userId,
                ExerciseName = model.ExerciseName.Trim(),
                Sets = model.Sets,
                Reps = model.Reps,
                WeightKg = model.WeightKg,
                LoggedAt = DateTime.Now
            });

            await db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Workout logged.";
            return RedirectToAction("WorkoutHistory");
        }

        // POST: /Fitness/LogSession - bulk-save every exercise result from a
        // guided workout session (see Views/Exercise/Index.cshtml). Accepts a
        // JSON array in the request body.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> LogSession(string exercisesJson)
        {
            List<SessionExerciseResultViewModel> exercises;
            try
            {
                exercises = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SessionExerciseResultViewModel>>(exercisesJson);
            }
            catch
            {
                return Json(new { success = false, message = "Could not read the session data." });
            }

            if (exercises == null || !exercises.Any())
            {
                return Json(new { success = false, message = "No exercises to save." });
            }

            var userId = User.Identity.GetUserId();
            var now = DateTime.Now;

            foreach (var ex in exercises)
            {
                if (string.IsNullOrWhiteSpace(ex.ExerciseName)) continue;

                db.WorkoutLogs.Add(new WorkoutLog
                {
                    UserId = userId,
                    ExerciseName = ex.ExerciseName.Trim(),
                    Sets = Math.Max(1, ex.Sets),
                    Reps = Math.Max(0, ex.Reps),
                    WeightKg = ex.WeightKg,
                    LoggedAt = now
                });
            }

            await db.SaveChangesAsync();

            return Json(new { success = true, message = "Session saved - " + exercises.Count + " exercise(s) logged." });
        }

        // GET: /Fitness/WorkoutHistory
        public async Task<ActionResult> WorkoutHistory()
        {
            var userId = User.Identity.GetUserId();

            var logs = await db.WorkoutLogs
                .Where(w => w.UserId == userId)
                .OrderByDescending(w => w.LoggedAt)
                .Take(50)
                .Select(w => new WorkoutLogItemViewModel
                {
                    ExerciseName = w.ExerciseName,
                    Sets = w.Sets,
                    Reps = w.Reps,
                    WeightKg = w.WeightKg,
                    LoggedAt = w.LoggedAt
                })
                .ToListAsync();

            return View(logs);
        }

        // GET: /Fitness/MealBuilder
        public async Task<ActionResult> MealBuilder()
        {
            var userId = User.Identity.GetUserId();

            var goal = await db.FitnessGoals
                .Where(g => g.UserId == userId && g.IsActive)
                .OrderByDescending(g => g.StartDate)
                .FirstOrDefaultAsync();

            ViewBag.Ingredients = NutritionGuide.Ingredients();
            ViewBag.GoalType = goal?.GoalType;
            ViewBag.GoalLabel = goal == null ? null :
                (goal.GoalType == "LoseWeight" ? "Lose Weight" :
                 goal.GoalType == "GainWeight" ? "Gain Weight" : "Maintain Weight");

            return View();
        }

        // GET: /Fitness
        public async Task<ActionResult> Index()
        {
            var userId = User.Identity.GetUserId();

            var history = await db.BodyMeasurements
                .Where(m => m.UserId == userId)
                .OrderByDescending(m => m.RecordedAt)
                .ToListAsync();

            var latest = history.FirstOrDefault();

            var goal = await db.FitnessGoals
                .Where(g => g.UserId == userId && g.IsActive)
                .OrderByDescending(g => g.StartDate)
                .FirstOrDefaultAsync();

            var model = new FitnessDashboardViewModel
            {
                HasMeasurements = latest != null,
                CurrentWeightKg = latest?.WeightKg,
                CurrentHeightCm = latest?.HeightCm,
                CurrentBmi = latest?.Bmi,
                BmiCategory = latest != null ? BodyMeasurement.BmiCategory(latest.Bmi) : null,
                LastRecordedAt = latest?.RecordedAt,
                History = history.Take(20).Select(m => new MeasurementHistoryItem
                {
                    RecordedAt = m.RecordedAt,
                    WeightKg = m.WeightKg,
                    HeightCm = m.HeightCm,
                    Bmi = m.Bmi
                }).ToList()
            };

            // "How far you are after a month" - compare the latest reading to the closest
            // one taken at least ~30 days earlier.
            if (latest != null)
            {
                var monthAgoCutoff = latest.RecordedAt.AddDays(-30);
                var monthAgoReading = history
                    .Where(m => m.RecordedAt <= monthAgoCutoff)
                    .OrderByDescending(m => m.RecordedAt)
                    .FirstOrDefault();

                if (monthAgoReading != null)
                {
                    model.HasMonthAgoMeasurement = true;
                    model.WeightMonthAgoKg = monthAgoReading.WeightKg;
                    model.WeightChangeKg = latest.WeightKg - monthAgoReading.WeightKg;
                }
            }

            if (goal != null)
            {
                model.HasActiveGoal = true;
                model.GoalType = goal.GoalType;
                model.StartWeightKg = goal.StartWeightKg;
                model.TargetWeightKg = goal.TargetWeightKg;
                model.GoalStartDate = goal.StartDate;

                var currentWeight = latest?.WeightKg ?? goal.StartWeightKg;
                var totalToChange = goal.StartWeightKg - goal.TargetWeightKg; // positive if losing weight
                var changedSoFar = goal.StartWeightKg - currentWeight;

                if (totalToChange == 0)
                {
                    model.ProgressPercent = 100;
                }
                else
                {
                    var pct = (changedSoFar / totalToChange) * 100m;
                    model.ProgressPercent = Math.Max(0, Math.Min(100, Math.Round(pct, 0)));
                }

                model.RemainingKg = Math.Abs(currentWeight - goal.TargetWeightKg);

                model.MealPlan = NutritionGuide.For(goal.GoalType);
            }

            return View(model);
        }

        // GET: /Fitness/LogMeasurement
        public async Task<ActionResult> LogMeasurement()
        {
            var userId = User.Identity.GetUserId();
            var latest = await db.BodyMeasurements
                .Where(m => m.UserId == userId)
                .OrderByDescending(m => m.RecordedAt)
                .FirstOrDefaultAsync();

            var model = new LogMeasurementViewModel();
            if (latest != null)
            {
                // Pre-fill height, since it rarely changes between weigh-ins
                model.HeightCm = latest.HeightCm;
            }

            return View(model);
        }

        // POST: /Fitness/LogMeasurement
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> LogMeasurement(LogMeasurementViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = User.Identity.GetUserId();

            var measurement = new BodyMeasurement
            {
                UserId = userId,
                WeightKg = model.WeightKg,
                HeightCm = model.HeightCm,
                RecordedAt = DateTime.Now,
                Bmi = BodyMeasurement.CalculateBmi(model.WeightKg, model.HeightCm)
            };

            db.BodyMeasurements.Add(measurement);
            await db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Measurement logged. BMI: " + measurement.Bmi + " (" + BodyMeasurement.BmiCategory(measurement.Bmi) + ").";
            return RedirectToAction("Index");
        }

        // GET: /Fitness/SetGoal
        public async Task<ActionResult> SetGoal()
        {
            var userId = User.Identity.GetUserId();
            var latest = await db.BodyMeasurements
                .Where(m => m.UserId == userId)
                .OrderByDescending(m => m.RecordedAt)
                .FirstOrDefaultAsync();

            var model = new SetGoalViewModel
            {
                GoalType = "LoseWeight",
                StartWeightKg = latest?.WeightKg ?? 0
            };

            return View(model);
        }

        // POST: /Fitness/SetGoal
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SetGoal(SetGoalViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = User.Identity.GetUserId();

            // Deactivate any previous goal - only one active goal at a time
            var oldGoals = await db.FitnessGoals
                .Where(g => g.UserId == userId && g.IsActive)
                .ToListAsync();
            foreach (var old in oldGoals)
            {
                old.IsActive = false;
            }

            var goal = new FitnessGoal
            {
                UserId = userId,
                GoalType = model.GoalType,
                StartWeightKg = model.StartWeightKg,
                TargetWeightKg = model.TargetWeightKg,
                StartDate = DateTime.Now,
                IsActive = true
            };

            db.FitnessGoals.Add(goal);
            await db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Goal set! We'll track your progress from here.";
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}

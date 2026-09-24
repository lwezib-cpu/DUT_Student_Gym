using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GymNet.ViewModels
{
    public class FitnessDashboardViewModel
    {
        public bool HasMeasurements { get; set; }

        // Latest reading
        public decimal? CurrentWeightKg { get; set; }
        public decimal? CurrentHeightCm { get; set; }
        public decimal? CurrentBmi { get; set; }
        public string BmiCategory { get; set; }
        public DateTime? LastRecordedAt { get; set; }

        // Reading from ~30 days ago, for the "progress after a month" view
        public bool HasMonthAgoMeasurement { get; set; }
        public decimal? WeightMonthAgoKg { get; set; }
        public decimal? WeightChangeKg { get; set; } // negative = lost weight

        // Active goal
        public bool HasActiveGoal { get; set; }
        public string GoalType { get; set; }
        public decimal? StartWeightKg { get; set; }
        public decimal? TargetWeightKg { get; set; }
        public DateTime? GoalStartDate { get; set; }
        public decimal? ProgressPercent { get; set; } // 0-100, toward the goal
        public decimal? RemainingKg { get; set; }

        public List<MeasurementHistoryItem> History { get; set; } = new List<MeasurementHistoryItem>();

        // Meal recommendations, populated when the member has an active goal.
        public GymNet.Helpers.MealPlan MealPlan { get; set; }
    }

    public class MeasurementHistoryItem
    {
        public DateTime RecordedAt { get; set; }
        public decimal WeightKg { get; set; }
        public decimal HeightCm { get; set; }
        public decimal Bmi { get; set; }
    }

    public class LogMeasurementViewModel
    {
        [Required(ErrorMessage = "Weight is required")]
        [Range(20, 300, ErrorMessage = "Enter a realistic weight in kg (20-300)")]
        [Display(Name = "Weight (kg)")]
        public decimal WeightKg { get; set; }

        [Required(ErrorMessage = "Height is required")]
        [Range(50, 250, ErrorMessage = "Enter a realistic height in cm (50-250)")]
        [Display(Name = "Height (cm)")]
        public decimal HeightCm { get; set; }
    }

    public class SetGoalViewModel
    {
        [Required(ErrorMessage = "Please choose a goal")]
        [Display(Name = "Goal")]
        public string GoalType { get; set; }

        [Required(ErrorMessage = "Current weight is required")]
        [Range(20, 300, ErrorMessage = "Enter a realistic weight in kg (20-300)")]
        [Display(Name = "Current Weight (kg)")]
        public decimal StartWeightKg { get; set; }

        [Required(ErrorMessage = "Target weight is required")]
        [Range(20, 300, ErrorMessage = "Enter a realistic weight in kg (20-300)")]
        [Display(Name = "Target Weight (kg)")]
        public decimal TargetWeightKg { get; set; }
    }

    public class LogWorkoutViewModel
    {
        [Required(ErrorMessage = "Exercise name is required")]
        [StringLength(100)]
        [Display(Name = "Exercise")]
        public string ExerciseName { get; set; }

        [Required]
        [Range(1, 50, ErrorMessage = "Enter a realistic number of sets")]
        public int Sets { get; set; } = 3;

        [Required]
        [Range(1, 200, ErrorMessage = "Enter a realistic number of reps")]
        public int Reps { get; set; } = 10;

        [Range(0, 500, ErrorMessage = "Enter a realistic weight in kg")]
        [Display(Name = "Weight (kg) - leave blank for bodyweight")]
        public decimal? WeightKg { get; set; }
    }

    public class WorkoutLogItemViewModel
    {
        public string ExerciseName { get; set; }
        public int Sets { get; set; }
        public int Reps { get; set; }
        public decimal? WeightKg { get; set; }
        public DateTime LoggedAt { get; set; }
    }

    // A single exercise result submitted from a guided workout session (see
    // Views/Exercise/Index.cshtml). Sent as JSON from the "Start Session" flow.
    public class SessionExerciseResultViewModel
    {
        public string ExerciseName { get; set; }
        public int Sets { get; set; }
        public int Reps { get; set; }
        public decimal? WeightKg { get; set; }
    }
}

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymNet.Models
{
    /// <summary>
    /// One weigh-in / measurement entry. A member can log as many of these
    /// as they like; the latest one is "current", and older ones let us
    /// show progress over time (e.g. "a month ago vs now").
    /// </summary>
    public class BodyMeasurement
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        public DateTime RecordedAt { get; set; } = DateTime.Now;

        [Range(20, 300, ErrorMessage = "Enter a realistic weight in kg")]
        public decimal WeightKg { get; set; }

        [Range(50, 250, ErrorMessage = "Enter a realistic height in cm")]
        public decimal HeightCm { get; set; }

        // BMI = weight(kg) / (height(m))^2 - stored so history doesn't need recalculating.
        public decimal Bmi { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        public static decimal CalculateBmi(decimal weightKg, decimal heightCm)
        {
            if (heightCm <= 0) return 0;
            var heightM = heightCm / 100m;
            return Math.Round(weightKg / (heightM * heightM), 1);
        }

        public static string BmiCategory(decimal bmi)
        {
            if (bmi <= 0) return "Unknown";
            if (bmi < 18.5m) return "Underweight";
            if (bmi < 25m) return "Normal";
            if (bmi < 30m) return "Overweight";
            return "Obese";
        }
    }
}

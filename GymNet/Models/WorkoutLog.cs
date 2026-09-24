using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymNet.Models
{
    /// <summary>
    /// A member's own record of a set they actually did - separate from the
    /// static exercise recommendations, this is a personal training log.
    /// </summary>
    public class WorkoutLog
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string ExerciseName { get; set; }

        [Range(1, 50)]
        public int Sets { get; set; }

        [Range(1, 200)]
        public int Reps { get; set; }

        // Null for bodyweight exercises (push-ups, plank, etc.)
        public decimal? WeightKg { get; set; }

        public DateTime LoggedAt { get; set; } = DateTime.Now;

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }
    }
}

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymNet.Models
{
    /// <summary>
    /// A member's current weight goal. Only one active goal per member -
    /// setting a new one replaces the old one (IsActive = false).
    /// </summary>
    public class FitnessGoal
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        [StringLength(20)]
        public string GoalType { get; set; } // "LoseWeight", "GainWeight", "MaintainWeight"

        [Range(20, 300)]
        public decimal StartWeightKg { get; set; }

        [Range(20, 300)]
        public decimal TargetWeightKg { get; set; }

        public DateTime StartDate { get; set; } = DateTime.Now;

        public bool IsActive { get; set; } = true;

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }
    }
}

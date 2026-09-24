using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymNet.Models
{
    public class TrainerFeedback
    {
        public int Id { get; set; }

        [Required]
        public string TrainerId { get; set; }

        [Required]
        public string MemberId { get; set; }

        [Required]
        [StringLength(1000)]
        public string Comment { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; } = 5;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("TrainerId")]
        public virtual ApplicationUser Trainer { get; set; }

        [ForeignKey("MemberId")]
        public virtual ApplicationUser Member { get; set; }
    }
}

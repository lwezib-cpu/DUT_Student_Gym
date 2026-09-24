using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymNet.Models
{
    public class GymClass
    {
        public int Id { get; set; }

        [Required]
        public string TrainerId { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        public DateTime StartTime { get; set; }

        public int DurationMinutes { get; set; } = 60;

        public int Capacity { get; set; } = 15;

        [StringLength(20)]
        public string Status { get; set; } = "Scheduled"; // Scheduled, Cancelled, Completed

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("TrainerId")]
        public virtual ApplicationUser Trainer { get; set; }

        public virtual ICollection<ClassBooking> Bookings { get; set; }
    }
}

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymNet.Models
{
    public class ClassBooking
    {
        public int Id { get; set; }

        public int GymClassId { get; set; }

        [Required]
        public string UserId { get; set; }

        public DateTime BookedAt { get; set; } = DateTime.Now;

        [StringLength(20)]
        public string Status { get; set; } = "Booked"; // Booked, Cancelled

        [ForeignKey("GymClassId")]
        public virtual GymClass GymClass { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }
    }
}

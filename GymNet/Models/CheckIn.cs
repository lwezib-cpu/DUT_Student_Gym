using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymNet.Models
{
    public class CheckIn
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        public DateTime CheckInTime { get; set; } = DateTime.Now;

        [StringLength(50)]
        public string CheckInMethod { get; set; } = "QR Code"; // QR Code, Manual

        [StringLength(100)]
        public string QRCode { get; set; } // The QR code that was scanned

        [StringLength(200)]
        public string Notes { get; set; }

        // Set when the member checks out (or is auto-checked-out). Null while still in the gym.
        public DateTime? CheckOutTime { get; set; }

        // Minutes spent in the gym for this visit, computed at check-out time.
        public int? DurationMinutes { get; set; }

        // Navigation property
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }
    }
}
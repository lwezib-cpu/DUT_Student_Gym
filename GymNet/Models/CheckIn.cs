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

        // Navigation property
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }
    }
}
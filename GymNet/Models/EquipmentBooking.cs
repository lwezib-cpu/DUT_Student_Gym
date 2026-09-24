using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymNet.Models
{
    public class EquipmentBooking
    {
        public int Id { get; set; }

        public int EquipmentId { get; set; }

        [Required]
        public string UserId { get; set; }

        public DateTime ReservedAt { get; set; } = DateTime.Now;

        public int ExpectedDurationMinutes { get; set; }

        public DateTime ExpectedReturnAt { get; set; }

        public DateTime? ActualReturnAt { get; set; }

        // Snapshot of the fees at the time this booking was made/closed, so changing
        // Equipment's prices later doesn't rewrite history.
        [DataType(DataType.Currency)]
        public decimal ReservationFeeCharged { get; set; }

        [DataType(DataType.Currency)]
        public decimal OverageFeeCharged { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = "PendingApproval"; // PendingApproval, Declined, Cancelled, Reserved, Returned

        // Shown to the member when admin declines their request.
        [StringLength(300)]
        public string DeclineReason { get; set; }

        [ForeignKey("EquipmentId")]
        public virtual Equipment Equipment { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }
    }
}

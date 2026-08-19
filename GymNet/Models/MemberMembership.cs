using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymNet.Models
{
    public class MemberMembership
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public int MembershipPlanId { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }
        public bool ReminderSent { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } // Active, Expired, Cancelled, PendingPayment

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        [ForeignKey("MembershipPlanId")]
        public virtual MembershipPlan MembershipPlan { get; set; }

        public virtual ICollection<Payment> Payments { get; set; }
    }
}
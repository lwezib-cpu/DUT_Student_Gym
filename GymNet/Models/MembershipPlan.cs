using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GymNet.Models
{
    public class MembershipPlan
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        public int DurationInMonths { get; set; } // 6 for semester

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation property
        public virtual ICollection<MemberMembership> MemberMemberships { get; set; }
    }
}
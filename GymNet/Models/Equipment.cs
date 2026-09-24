using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GymNet.Models
{
    public class Equipment
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(300)]
        public string Description { get; set; }

        public int TotalQuantity { get; set; } = 1;

        // Charged upfront when a member reserves this equipment.
        [DataType(DataType.Currency)]
        public decimal ReservationFee { get; set; }

        // Charged per hour (or part thereof) that a member keeps the equipment past
        // their expected return time.
        [DataType(DataType.Currency)]
        public decimal OveragePerHourFee { get; set; }

        public bool IsActive { get; set; } = true;

        // Admin can take a specific unit out of the booking pool for repairs.
        public bool IsUnderMaintenance { get; set; }

        [StringLength(200)]
        public string MaintenanceNote { get; set; }

        public virtual ICollection<EquipmentBooking> Bookings { get; set; }
    }
}

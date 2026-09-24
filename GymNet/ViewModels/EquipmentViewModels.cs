using System;
using System.ComponentModel.DataAnnotations;

namespace GymNet.ViewModels
{
    public class EquipmentListItemViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int TotalQuantity { get; set; }
        public int AvailableNow { get; set; }
        public decimal ReservationFee { get; set; }
        public decimal OveragePerHourFee { get; set; }

        // Simple animated icon shown on the card - see Views/Equipment/Index.cshtml
        // for why this is a CSS icon rather than a real photo/GIF.
        public string IconClass { get; set; }
        public string AnimationClass { get; set; }
        public string LottieUrl { get; set; }

        public bool IsUnderMaintenance { get; set; }
        public string MaintenanceNote { get; set; }

        // Local photo (Content/images/equipment/...). When set, the card shows this
        // real photo instead of the icon/Lottie fallback.
        public string ImageUrl { get; set; }
    }

    public class ReserveEquipmentViewModel
    {
        [Required]
        public int EquipmentId { get; set; }

        [Required]
        [Range(15, 240, ErrorMessage = "Choose a duration between 15 and 240 minutes")]
        [Display(Name = "Expected Duration (minutes)")]
        public int ExpectedDurationMinutes { get; set; } = 60;
    }

    public class EquipmentBookingAdminListItemViewModel
    {
        public int Id { get; set; }
        public string EquipmentName { get; set; }
        public string EquipmentImageUrl { get; set; }
        public string EquipmentIconClass { get; set; }
        public string MemberName { get; set; }
        public string MemberEmail { get; set; }
        public DateTime ReservedAt { get; set; }
        public int ExpectedDurationMinutes { get; set; }
        public DateTime ExpectedReturnAt { get; set; }
        public DateTime? ActualReturnAt { get; set; }
        public decimal ReservationFeeCharged { get; set; }
        public decimal OverageFeeCharged { get; set; }
        public string Status { get; set; }
        public string DeclineReason { get; set; }
    }

    public class EquipmentBookingListItemViewModel
    {
        public int Id { get; set; }
        public string EquipmentName { get; set; }
        public DateTime ReservedAt { get; set; }
        public DateTime ExpectedReturnAt { get; set; }
        public DateTime? ActualReturnAt { get; set; }
        public decimal ReservationFeeCharged { get; set; }
        public decimal OverageFeeCharged { get; set; }
        public string Status { get; set; }
        public string DeclineReason { get; set; }
    }
}

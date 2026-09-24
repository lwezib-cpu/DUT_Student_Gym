using System;

namespace GymNet.ViewModels
{
    public class MemberDashboardViewModel
    {
        // Profile Info
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string Gender { get; set; }
        public DateTime DateOfBirth { get; set; }

        // Membership Info
        public string MembershipStatus { get; set; }
        public string MembershipPlan { get; set; }
        public DateTime? MembershipEndDate { get; set; }
        public int DaysRemaining { get; set; }
        public bool HasActiveMembership { get; set; }
        public DateTime? MemberSince { get; set; }

        // Payment Info
        public string PaymentStatus { get; set; }
        public decimal TotalPayments { get; set; }
        public DateTime? LastPaymentDate { get; set; }
        public decimal LastPaymentAmount { get; set; }

        // Currently checked in (for a visible Check Out shortcut on the dashboard)
        public bool IsCheckedInNow { get; set; }
        public DateTime? OpenCheckInTime { get; set; }

        // Consistency badge
        public int TotalCheckIns { get; set; }

        // Expiry reminder - true when membership is active but running out soon
        public bool IsExpiringSoon => HasActiveMembership && DaysRemaining > 0 && DaysRemaining <= 7;
        public string BadgeName { get; set; }
        public string BadgeCssClass { get; set; }
        public string BadgeIcon { get; set; }
        public int BadgeNextThreshold { get; set; }
        public int BadgeProgressPercent { get; set; }
    }

    public class LeaderboardEntryViewModel
    {
        public int Rank { get; set; }
        public string FullName { get; set; }
        public int TotalCheckIns { get; set; }
        public string BadgeName { get; set; }
        public string BadgeCssClass { get; set; }
        public string BadgeIcon { get; set; }
        public bool IsCurrentUser { get; set; }
    }
}
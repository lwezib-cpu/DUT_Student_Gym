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
    }
}
using System;

namespace GymNet.ViewModels
{
   

    public class MemberDashboardViewModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string Gender { get; set; }
        public DateTime DateOfBirth { get; set; }

        // Placeholders — Membership & Payment modules aren't built yet
        public string MembershipStatus { get; set; }
        public string MembershipPlan { get; set; }
        public string PaymentStatus { get; set; }
    }
}
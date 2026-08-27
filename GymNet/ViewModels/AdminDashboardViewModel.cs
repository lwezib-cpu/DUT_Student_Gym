using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GymNet.ViewModels
{
    public class AdminDashboardViewModel
    {
        public string AdminName { get; set; }
        public int TotalMembers { get; set; }
        public int ActiveMemberships { get; set; }
        public int TotalMembershipPlans { get; set; }
        public int TotalPayments { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class MemberListItemViewModel
    {
        public string UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public bool IsActive { get; set; }
        public DateTime? MemberSince { get; set; }
        public string MembershipStatus { get; set; }
        public string MembershipPlan { get; set; }
        public bool HasPaid { get; set; }
    }

    public class AdminMemberDetailsViewModel
    {
        public string UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Gender { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Address { get; set; }
        public bool IsActive { get; set; }
        public List<MemberMembershipHistoryViewModel> Memberships { get; set; }
        public List<MemberPaymentHistoryViewModel> Payments { get; set; }
    }
    public class ManualCheckInMemberViewModel
    {
        public string UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public bool HasActiveMembership { get; set; }
        public string MembershipStatus { get; set; }
        public string MembershipPlan { get; set; }
        public bool HasPaid { get; set; }
        public bool CheckedInToday { get; set; }
    }
    public class EditMemberViewModel
    {
        public string UserId { get; set; }

        [Required(ErrorMessage = "First name is required")]
        [StringLength(50)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Enter a valid email address")]
        [Display(Name = "Email Address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Enter a valid phone number")]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Date of birth is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Gender is required")]
        [Display(Name = "Gender")]
        public string Gender { get; set; }

        [Required(ErrorMessage = "Address is required")]
        [StringLength(200)]
        [Display(Name = "Address")]
        public string Address { get; set; }
    }

    public class MemberMembershipHistoryViewModel
    {
        public int Id { get; set; }
        public string PlanName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; }
    }

    public class MemberPaymentHistoryViewModel
    {
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; }
        public DateTime PaymentDate { get; set; }
        public string Status { get; set; }
        public string TransactionReference { get; set; }
    }
    public class AdminRegisterMemberViewModel
    {
        [Required(ErrorMessage = "First name is required")]
        [StringLength(50)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Enter a valid email address")]
        [Display(Name = "Email Address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Enter a valid phone number")]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Date of birth is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Gender is required")]
        [Display(Name = "Gender")]
        public string Gender { get; set; }

        [Required(ErrorMessage = "Address is required")]
        [StringLength(200)]
        [Display(Name = "Address")]
        public string Address { get; set; }

        [Display(Name = "Assign Membership Plan")]
        public int? MembershipPlanId { get; set; }

        [Display(Name = "Collect Payment Now")]
        public bool CollectPaymentNow { get; set; }
    }
    public class AdminProcessPaymentViewModel
    {
        public string UserId { get; set; }
        public string MemberName { get; set; }
        public string Email { get; set; }
        public int? MembershipId { get; set; }
        public string MembershipPlanName { get; set; }
        public decimal AmountDue { get; set; }
        public string MembershipStatus { get; set; }
        public DateTime? MembershipEndDate { get; set; }
        public bool HasPendingPayment { get; set; }
    }

    public class AdminPaymentListViewModel
    {
        public int PaymentId { get; set; }
        public string MemberName { get; set; }
        public string Email { get; set; }
        public string PlanName { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; }
        public DateTime PaymentDate { get; set; }
        public string Status { get; set; }
        public string TransactionReference { get; set; }
    }

    public class AdminRecordPaymentViewModel
    {
        [Required]
        public string UserId { get; set; }

        [Required(ErrorMessage = "Membership is required")]
        public int MemberMembershipId { get; set; }

        [Required(ErrorMessage = "Amount is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        [DataType(DataType.Currency)]
        [Display(Name = "Payment Amount")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Payment method is required")]
        [Display(Name = "Payment Method")]
        public string PaymentMethod { get; set; }

        [Display(Name = "Transaction Reference (Optional)")]
        public string TransactionReference { get; set; }

        [Display(Name = "Notes (Optional)")]
        public string Notes { get; set; }
    }
}
using System;
using System.ComponentModel.DataAnnotations;

namespace GymNet.ViewModels
{
    public class MembershipPlanViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int DurationInMonths { get; set; }
        public bool IsActive { get; set; }
    }

    public class SelectMembershipViewModel
    {
        public int PlanId { get; set; }
        public string PlanName { get; set; }
        public decimal Price { get; set; }
        public int DurationInMonths { get; set; }
    }

    public class PaymentViewModel
    {
        [Required]
        public int MembershipPlanId { get; set; }

        [Required(ErrorMessage = "Payment method is required")]
        [Display(Name = "Payment Method")]
        public string PaymentMethod { get; set; }

        // Credit/Debit Card Fields
        [Display(Name = "Card Holder Name")]
        [StringLength(100, ErrorMessage = "Card holder name cannot exceed 100 characters")]
        public string CardHolderName { get; set; }

        [Display(Name = "Card Number")]
        [StringLength(19, MinimumLength = 16, ErrorMessage = "Card number must be 16 digits")]
        public string CardNumber { get; set; }

        [Display(Name = "Expiry Date (MM/YY)")]
        [StringLength(5, ErrorMessage = "Expiry date must be in MM/YY format")]
        public string ExpiryDate { get; set; }

        [Display(Name = "CVV")]
        [StringLength(4, MinimumLength = 3, ErrorMessage = "CVV must be 3 or 4 digits")]
        public string CVV { get; set; }

        // EFT Fields
        [Display(Name = "Bank Name")]
        [StringLength(100)]
        public string BankName { get; set; }

        [Display(Name = "Account Holder")]
        [StringLength(100)]
        public string AccountHolderName { get; set; }

        [Display(Name = "Account Number")]
        [StringLength(20, MinimumLength = 8, ErrorMessage = "Account number must be between 8 and 20 digits")]
        public string AccountNumber { get; set; }

        [Display(Name = "Branch Code")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Branch code must be 6 digits")]
        public string BranchCode { get; set; }

        [Display(Name = "Account Type")]
        public string AccountType { get; set; } // Cheque, Savings, Transmission
    }

    public class MemberMembershipViewModel
    {
        public int Id { get; set; }
        public string PlanName { get; set; }
        public string PlanDescription { get; set; }
        public decimal Price { get; set; }
        public int DurationInMonths { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; }
        public int DaysRemaining { get; set; }

        // Payment Info
        public string PaymentStatus { get; set; }
        public DateTime? LastPaymentDate { get; set; }
        public decimal LastPaymentAmount { get; set; }
        public string TransactionReference { get; set; }
        public string PaymentMethod { get; set; }
        public decimal TotalPaid { get; set; }
    }

    public class PaymentHistoryViewModel
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; }
        public DateTime PaymentDate { get; set; }
        public string Status { get; set; }
        public string TransactionReference { get; set; }
        public string CardHolderName { get; set; }
        public string MaskedCardNumber { get; set; }
        public string BankName { get; set; }
        public string MembershipPlanName { get; set; }
        public DateTime MembershipStartDate { get; set; }
        public DateTime MembershipEndDate { get; set; }

        // Additional payment details for display
        public string AccountNumber { get; set; }
        public string AccountType { get; set; }
        public string BranchCode { get; set; }
    }

    public class ReceiptViewModel
    {
        public int PaymentId { get; set; }
        public string TransactionReference { get; set; }
        public DateTime PaymentDate { get; set; }
        public string MembershipPlanName { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; }
        public string Status { get; set; }
        public string BankName { get; set; }
        public string CardHolderName { get; set; }
        public string MaskedCardNumber { get; set; }
        public DateTime MembershipStartDate { get; set; }
        public DateTime MembershipEndDate { get; set; }

        // EFT specific fields
        public string AccountNumber { get; set; }
        public string AccountType { get; set; }
        public string BranchCode { get; set; }

        // Member details
        public string MemberName { get; set; }
        public string MemberEmail { get; set; }

        // Gym details (you can move these to a settings/config file)
        public string GymName { get; set; } = "GymNet Fitness Center";
        public string GymAddress { get; set; } = "123 Fitness Street, Johannesburg, South Africa";
        public string GymPhone { get; set; } = "+27 11 234 5678";
        public string GymEmail { get; set; } = "info@gymnet.co.za";

        // Computed properties for display
        public string FormattedAmount => $"R {Amount:F2}";
        public string SubscriptionPeriod => $"{MembershipStartDate:dd MMM yyyy} - {MembershipEndDate:dd MMM yyyy}";
        public int SubscriptionDurationDays => (MembershipEndDate - MembershipStartDate).Days;
        public bool IsActive => MembershipEndDate > DateTime.Now;
        public int DaysRemaining => IsActive ? (MembershipEndDate - DateTime.Now).Days : 0;
    }
}
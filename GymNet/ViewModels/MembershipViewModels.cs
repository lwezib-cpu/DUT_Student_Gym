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

        [Required(ErrorMessage = "Card holder name is required")]
        [Display(Name = "Card Holder Name")]
        [StringLength(100)]
        public string CardHolderName { get; set; }

        [Required(ErrorMessage = "Card number is required")]
        [Display(Name = "Card Number")]
        [StringLength(19, MinimumLength = 16)]
        [RegularExpression(@"^\d{16}$", ErrorMessage = "Please enter a valid 16-digit card number")]
        public string CardNumber { get; set; }

        [Required(ErrorMessage = "Expiry date is required")]
        [Display(Name = "Expiry Date (MM/YY)")]
        [StringLength(5)]
        [RegularExpression(@"^(0[1-9]|1[0-2])\/([0-9]{2})$", ErrorMessage = "Please use MM/YY format")]
        public string ExpiryDate { get; set; }

        [Required(ErrorMessage = "CVV is required")]
        [Display(Name = "CVV")]
        [StringLength(4, MinimumLength = 3)]
        [RegularExpression(@"^\d{3,4}$", ErrorMessage = "Please enter a valid CVV")]
        public string CVV { get; set; }

        [Required(ErrorMessage = "Bank name is required")]
        [Display(Name = "Bank Name")]
        [StringLength(100)]
        public string BankName { get; set; }

        [Required(ErrorMessage = "Account number is required")]
        [Display(Name = "Account Number")]
        [StringLength(20)]
        [RegularExpression(@"^\d{8,20}$", ErrorMessage = "Please enter a valid account number")]
        public string AccountNumber { get; set; }
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
    }
}
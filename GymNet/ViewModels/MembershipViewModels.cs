using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

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

        // Plans of 1 month or less (Monthly) are never locked to the semester calendar.
        public bool IsLocked { get; set; }
    }

    public class SelectMembershipViewModel
    {
        public int PlanId { get; set; }
        public string PlanName { get; set; }
        public decimal Price { get; set; }
        public int DurationInMonths { get; set; }
    }

    // Card fields are only required when PaymentMethod is Credit/Debit Card; bank fields
    // are only required for EFT. Plain [Required] on all of them was the bug that made
    // the demo payment silently fail - e.g. picking Credit Card and leaving Bank Name
    // blank (a field that isn't even shown for that method) failed server validation.
    public class PaymentViewModel : IValidatableObject
    {
        [Required]
        public int MembershipPlanId { get; set; }

        [Required(ErrorMessage = "Payment method is required")]
        [Display(Name = "Payment Method")]
        public string PaymentMethod { get; set; }

        [Display(Name = "Card Holder Name")]
        [StringLength(100)]
        public string CardHolderName { get; set; }

        [Display(Name = "Card Number")]
        [StringLength(19)]
        public string CardNumber { get; set; }

        [Display(Name = "Expiry Date (MM/YY)")]
        [StringLength(5)]
        public string ExpiryDate { get; set; }

        [Display(Name = "CVV")]
        [StringLength(4)]
        public string CVV { get; set; }

        [Display(Name = "Bank Name")]
        [StringLength(100)]
        public string BankName { get; set; }

        [Display(Name = "Account Number")]
        [StringLength(20)]
        public string AccountNumber { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var isCard = PaymentMethod == "Credit Card" || PaymentMethod == "Debit Card";
            var isEft = PaymentMethod == "EFT";

            if (isCard)
            {
                if (string.IsNullOrWhiteSpace(CardHolderName))
                    yield return new ValidationResult("Card holder name is required", new[] { nameof(CardHolderName) });

                if (string.IsNullOrWhiteSpace(CardNumber) || !Regex.IsMatch(CardNumber, @"^\d{16}$"))
                    yield return new ValidationResult("Please enter a valid 16-digit card number", new[] { nameof(CardNumber) });

                if (string.IsNullOrWhiteSpace(ExpiryDate) || !Regex.IsMatch(ExpiryDate, @"^(0[1-9]|1[0-2])\/([0-9]{2})$"))
                    yield return new ValidationResult("Please use MM/YY format", new[] { nameof(ExpiryDate) });

                if (string.IsNullOrWhiteSpace(CVV) || !Regex.IsMatch(CVV, @"^\d{3,4}$"))
                    yield return new ValidationResult("Please enter a valid CVV", new[] { nameof(CVV) });
            }
            else if (isEft)
            {
                if (string.IsNullOrWhiteSpace(BankName))
                    yield return new ValidationResult("Bank name is required", new[] { nameof(BankName) });

                if (string.IsNullOrWhiteSpace(AccountNumber) || !Regex.IsMatch(AccountNumber, @"^\d{8,20}$"))
                    yield return new ValidationResult("Please enter a valid account number", new[] { nameof(AccountNumber) });
            }
        }
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

    public class MembershipCardViewModel
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PlanName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string MemberId { get; set; }
    }
}
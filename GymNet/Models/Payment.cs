using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymNet.Models
{
    public class Payment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Amount")]
        public decimal Amount { get; set; }

        [Required]
        [Display(Name = "Payment Method")]
        public string PaymentMethod { get; set; }

        [Required]
        [Display(Name = "Status")]
        public string Status { get; set; }

        [Required]
        [Display(Name = "Payment Date")]
        public DateTime PaymentDate { get; set; }

        [Display(Name = "Transaction Reference")]
        public string TransactionReference { get; set; }

        // Card fields
        [Display(Name = "Card Holder Name")]
        public string CardHolderName { get; set; }

        [Display(Name = "Card Number")]
        public string CardNumber { get; set; } // Masked

        [Display(Name = "Expiry Date")]
        public string ExpiryDate { get; set; }

        // EFT fields
        [Display(Name = "Bank Name")]
        public string BankName { get; set; }

        [Display(Name = "Account Number")]
        public string AccountNumber { get; set; } // Masked

        [Display(Name = "Account Type")]
        public string AccountType { get; set; } // Cheque, Savings, Transmission

        [Display(Name = "Branch Code")]
        public string BranchCode { get; set; }

        // Navigation properties
        [ForeignKey("MemberMembership")]
        public int MemberMembershipId { get; set; }
        public virtual MemberMembership MemberMembership { get; set; }
    }
}
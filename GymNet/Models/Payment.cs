using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymNet.Models
{
    public class Payment
    {
        public int Id { get; set; }

        public int MemberMembershipId { get; set; }

        [Required]
        [StringLength(50)]
        public string PaymentMethod { get; set; } // Credit Card, Debit Card, EFT

        [Required]
        [DataType(DataType.Currency)]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } // Completed, Pending, Failed

        public DateTime PaymentDate { get; set; } = DateTime.Now;

        // Demo payment fields
        [StringLength(100)]
        public string CardHolderName { get; set; }

        [StringLength(20)]
        public string CardNumber { get; set; } // Will be masked in display

        [StringLength(5)]
        public string ExpiryDate { get; set; }

        [StringLength(100)]
        public string BankName { get; set; }

        [StringLength(20)]
        public string AccountNumber { get; set; }

        [StringLength(50)]
        public string TransactionReference { get; set; }

        // Navigation property
        [ForeignKey("MemberMembershipId")]
        public virtual MemberMembership MemberMembership { get; set; }
    }
}
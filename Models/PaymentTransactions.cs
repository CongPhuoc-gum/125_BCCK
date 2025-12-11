using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace _125_BCCK.Models
{
    [Table("PaymentTransactions")]
    public class PaymentTransaction
    {
        [Key]
        public int TransactionId { get; set; }

        [Required]
        public int AppointmentId { get; set; }

        [Required, MaxLength(20)]
        public string TransactionType { get; set; } // 'Deposit' hoặc 'Final'

        [Required]
        public decimal Amount { get; set; }

        [MaxLength(50)]
        public string PaymentMethod { get; set; } // 'Cash', 'BankTransfer', 'Card', 'Momo', 'ZaloPay'

        [Required]
        public DateTime PaymentDate { get; set; }

        public int? ProcessedBy { get; set; } // StaffId xử lý thanh toán

        public string Notes { get; set; }

        // Navigation properties
        [ForeignKey("AppointmentId")]
        public virtual Appointment Appointment { get; set; }

        [ForeignKey("ProcessedBy")]
        public virtual User ProcessedByUser { get; set; }
    }
}
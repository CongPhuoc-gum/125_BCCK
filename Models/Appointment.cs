using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace _125_BCCK.Models
{
    [Table("Appointments")]
    public class Appointment
    {
        [Key]
        public int AppointmentId { get; set; }

        [Required]
        public int CustomerId { get; set; }

        [Required]
        public int PetId { get; set; }

        public int? StaffId { get; set; }

        [Required]
        public DateTime AppointmentDate { get; set; }

        [Required, MaxLength(20)]
        public string TimeSlot { get; set; }

        [Required, MaxLength(20)]
        public string Status { get; set; } // 'Pending', 'Confirmed', 'InProgress', 'Completed', 'Cancelled'

        public string CustomerNotes { get; set; }

        public string StaffNotes { get; set; }

        public string CancelReason { get; set; }

        // Thanh toán
        [Required]
        public decimal TotalPrice { get; set; }

        public decimal? DepositAmount { get; set; }

        public bool DepositPaid { get; set; }

        public decimal? RemainingAmount { get; set; }

        public bool FullyPaid { get; set; }

        [MaxLength(50)]
        public string PaymentMethod { get; set; } // 'Cash', 'BankTransfer', 'Card', 'Momo', 'ZaloPay'

        // Email
        public bool EmailSent { get; set; }

        public DateTime? EmailSentDate { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey("CustomerId")]
        public virtual User Customer { get; set; }

        [ForeignKey("PetId")]
        public virtual Pet Pet { get; set; }

        [ForeignKey("StaffId")]
        public virtual User Staff { get; set; }

        public virtual ICollection<AppointmentService> AppointmentServices { get; set; }
    }
}
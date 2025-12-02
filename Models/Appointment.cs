using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace _125_BCCK.Models
{
    [Table("Appointments")]
    public class Appointment
    {
        [Key]
        public int AppointmentId { get; set; }

        public int CustomerId { get; set; }
        public int PetId { get; set; }
        public int? StaffId { get; set; }

        public DateTime AppointmentDate { get; set; }

        [StringLength(20)]
        public string TimeSlot { get; set; }

        [StringLength(20)]
        public string Status { get; set; }

        public string CustomerNotes { get; set; }
        public string StaffNotes { get; set; }

        public decimal TotalPrice { get; set; }
        public decimal? DepositAmount { get; set; }
        public bool DepositPaid { get; set; }
        public decimal? RemainingAmount { get; set; }
        public bool FullyPaid { get; set; }
        public string PaymentMethod { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
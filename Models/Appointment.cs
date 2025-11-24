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
        public string Status { get; set; } // Pending, Completed...

        public decimal TotalPrice { get; set; }

        // Khóa ngoại
        [ForeignKey("CustomerId")]
        public virtual User Customer { get; set; }

        [ForeignKey("PetId")]
        public virtual Pet Pet { get; set; }

        [ForeignKey("StaffId")]
        public virtual User Staff { get; set; }
    }
}
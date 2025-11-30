using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace _125_BCCK.Models
{
    [Table("AppointmentServices")]
    public class AppointmentService
    {
        [Key]
        public int AppointmentServiceId { get; set; }

        [Required]
        public int AppointmentId { get; set; }

        [Required]
        public int ServiceId { get; set; }

        [Required]
        public decimal ServicePrice { get; set; }

        [ForeignKey("AppointmentId")]
        public virtual Appointment Appointment { get; set; }

        [ForeignKey("ServiceId")]
        public virtual Service Service { get; set; }
    }
}

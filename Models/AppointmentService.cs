using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace _125_BCCK.Models
{
    [Table("AppointmentServices")]
    public class AppointmentService
    {
        [Key]
        public int AppointmentServiceId { get; set; }

        public int AppointmentId { get; set; }
        public int ServiceId { get; set; }
        public decimal ServicePrice { get; set; }
    }
}
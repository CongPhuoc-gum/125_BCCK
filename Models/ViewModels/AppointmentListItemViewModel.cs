using System;

namespace _125_BCCK.Models.ViewModels
{
    public class AppointmentListItemViewModel
    {
        public int AppointmentId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string TimeSlot { get; set; }
        public string Status { get; set; }
        public string CustomerName { get; set; }
        public string PetName { get; set; }
        public string Species { get; set; }
        public string Services { get; set; }
        public decimal TotalPrice { get; set; }
        public string CustomerNotes { get; set; }
    }
}

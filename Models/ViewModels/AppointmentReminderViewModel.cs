using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace _125_BCCK.Models.ViewModels
{
    public class AppointmentReminderViewModel
    {
        public int AppointmentId { get; set; }
        public string TimeSlot { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string PetName { get; set; }
    }
}
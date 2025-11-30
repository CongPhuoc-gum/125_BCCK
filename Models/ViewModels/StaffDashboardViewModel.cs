using System;
using System.Collections.Generic;

namespace _125_BCCK.Models.ViewModels
{
    public class StaffDashboardViewModel
    {
        public int TodayAppointmentsCount { get; set; }
        public int WeekAppointmentsCount { get; set; }
        public List<AppointmentListItemViewModel> TodayAppointments { get; set; }
    }
}

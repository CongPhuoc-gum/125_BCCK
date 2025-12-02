using System;
using System.Collections.Generic;

namespace _125_BCCK.Models.ViewModels
{
    public class PetDetailsViewModel
    {
        public Pet Pet { get; set; }
        public List<VaccinationHistoryItem> VaccinationHistory { get; set; }
        public List<ServiceHistoryItem> ServiceHistory { get; set; }
        public PetStatistics Statistics { get; set; }
    }

    public class VaccinationHistoryItem
    {
        public string VaccineName { get; set; }
        public DateTime VaccinationDate { get; set; }
        public DateTime? NextDueDate { get; set; }
        public string Notes { get; set; }
        public string VaccinatedBy { get; set; }

        public int? DaysUntilNextDue
        {
            get
            {
                if (NextDueDate.HasValue)
                {
                    return (NextDueDate.Value - DateTime.Now).Days;
                }
                return null;
            }
        }

        public bool IsOverdue
        {
            get
            {
                return NextDueDate.HasValue && NextDueDate.Value < DateTime.Now;
            }
        }
    }

    public class ServiceHistoryItem
    {
        public int AppointmentId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string TimeSlot { get; set; }
        public string Status { get; set; }
        public string Services { get; set; }
        public decimal TotalPrice { get; set; }
        public bool FullyPaid { get; set; }
        public string StaffName { get; set; }

        public string StatusDisplay
        {
            get
            {
                switch (Status)
                {
                    case "Pending": return "Chờ xử lý";
                    case "Confirmed": return "Đã xác nhận";
                    case "InProgress": return "Đang thực hiện";
                    case "Completed": return "Hoàn thành";
                    case "Cancelled": return "Đã hủy";
                    default: return Status;
                }
            }
        }

        public string StatusColor
        {
            get
            {
                switch (Status)
                {
                    case "Pending": return "warning";
                    case "Confirmed": return "info";
                    case "InProgress": return "primary";
                    case "Completed": return "success";
                    case "Cancelled": return "danger";
                    default: return "secondary";
                }
            }
        }
    }

    public class PetStatistics
    {
        public int TotalVaccinations { get; set; }
        public int TotalAppointments { get; set; }
        public decimal TotalSpent { get; set; }
        public DateTime? LastVisit { get; set; }
        public int UpcomingVaccinations { get; set; }
    }
}
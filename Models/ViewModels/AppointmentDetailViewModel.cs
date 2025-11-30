using System;
using System.Collections.Generic;

namespace _125_BCCK.Models.ViewModels
{
    public class AppointmentDetailViewModel
    {
        public int AppointmentId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string TimeSlot { get; set; }
        public string Status { get; set; }
        public decimal TotalPrice { get; set; }
        public string CustomerNotes { get; set; }
        public string StaffNotes { get; set; }
        public string CancelReason { get; set; }
        public DateTime CreatedAt { get; set; }

        // Thông tin khách hàng
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerPhone { get; set; }
        public string CustomerAddress { get; set; }

        // Thông tin thú cưng
        public int PetId { get; set; }
        public string PetName { get; set; }
        public string Species { get; set; }
        public string Breed { get; set; }
        public int? Age { get; set; }
        public decimal? Weight { get; set; }
        public string Gender { get; set; }
        public string Color { get; set; }
        public string SpecialNotes { get; set; }

        // Dịch vụ
        public List<ServiceItemViewModel> Services { get; set; }
    }

    public class ServiceItemViewModel
    {
        public string ServiceName { get; set; }
        public string Category { get; set; }
        public int Duration { get; set; }
        public decimal Price { get; set; }
    }
}

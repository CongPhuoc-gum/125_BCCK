using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace _125_BCCK.Models.ViewModels
{
    public class BookingSuccessViewModel
    {
        public int AppointmentId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string TimeSlot { get; set; }
        public string Status { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal DepositAmount { get; set; }
        public bool DepositPaid { get; set; }
        public decimal RemainingAmount { get; set; }
        public string PetName { get; set; }
        public string Species { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public List<ServiceItemViewModel> Services { get; set; }
    }
}
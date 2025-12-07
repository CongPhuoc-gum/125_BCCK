using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace _125_BCCK.Models.ViewModels
{
    public class PaymentViewModel
    {
        public int PetId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string TimeSlot { get; set; }
        public string ServiceIds { get; set; }
        public string CustomerNotes { get; set; }
        public string PaymentMethod { get; set; }

        public decimal TotalPrice { get; set; }
        public decimal DepositAmount { get; set; }
        public decimal RemainingAmount { get; set; }

        public List<ServiceItemViewModel> Services { get; set; }
    }
}
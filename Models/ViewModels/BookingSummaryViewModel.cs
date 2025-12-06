using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace _125_BCCK.Models.ViewModels
{
    public class BookingSummaryViewModel
    {
        public bool Success { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal DepositAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public int TotalDuration { get; set; }
        public List<ServiceItemViewModel> Services { get; set; }
        public string Message { get; set; }
    }
}
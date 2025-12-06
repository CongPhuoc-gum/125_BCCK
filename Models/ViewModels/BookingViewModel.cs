using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace _125_BCCK.Models.ViewModels
{
    public class BookingViewModel
    {
        public List<Service> Services { get; set; }
        public List<Pet> Pets { get; set; }
        public List<string> TimeSlots { get; set; }
        public int? PreSelectedServiceId { get; set; }
        public bool NeedAddPet { get; set; }
    }
}
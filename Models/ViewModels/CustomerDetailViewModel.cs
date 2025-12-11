using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace _125_BCCK.Models.ViewModels
{
    public class CustomerDetailViewModel
    {
        public User Customer { get; set; }
        public List<Pet> Pets { get; set; }
        public List<Appointment> Appointments { get; set; }
    
    }

}
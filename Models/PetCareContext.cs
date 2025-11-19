using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing;
using System.Linq;
using System.Web;

namespace _125_BCCK.Models
{
    public class PetCareContext : DbContext
    {
        public PetCareContext() : base("name = PetCareDBEntities")
        {
            this.Configuration.LazyLoadingEnabled = false;

            this.Configuration.ProxyCreationEnabled = false;
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Service> Services { get; set; }
        //public DbSet<Pet> Pets { get; set; }
        //public DbSet<Appointment> Appointments { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
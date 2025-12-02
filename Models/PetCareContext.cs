using System.Data.Entity;

namespace _125_BCCK.Models
{
    public class PetCareContext : DbContext
    {
        public PetCareContext() : base("name=PetCareDBEntities")
        {
            Database.SetInitializer<PetCareContext>(null);

            this.Configuration.LazyLoadingEnabled = false;
            this.Configuration.ProxyCreationEnabled = false;
            this.Configuration.ValidateOnSaveEnabled = false;
        }

        // Khai báo DbSet
        public DbSet<Service> Services { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Pet> Pets { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<AppointmentService> AppointmentServices { get; set; }
        //public DbSet<PaymentTransaction> PaymentTransactions { get; set; }
        public DbSet<VaccinationRecord> VaccinationRecords { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cấu hình bảng
            modelBuilder.Entity<Service>().ToTable("Services");
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<Pet>().ToTable("Pets");
            modelBuilder.Entity<Appointment>().ToTable("Appointments");
            modelBuilder.Entity<AppointmentService>().ToTable("AppointmentServices");
            //modelBuilder.Entity<PaymentTransaction>().ToTable("PaymentTransactions");
            modelBuilder.Entity<VaccinationRecord>().ToTable("VaccinationRecords");
        }
    }
}
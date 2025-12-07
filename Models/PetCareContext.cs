using System;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;

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

        // ==================== DbSet Declarations ====================
        public DbSet<Service> Services { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Pet> Pets { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<AppointmentService> AppointmentServices { get; set; }
        //public DbSet<PaymentTransaction> PaymentTransactions { get; set; }
        public DbSet<VaccinationRecord> VaccinationRecords { get; set; }

        // ==================== Entity Configuration ====================
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Mapping entity sang table trong database
            modelBuilder.Entity<Service>().ToTable("Services");
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<Pet>().ToTable("Pets");
            modelBuilder.Entity<Appointment>().ToTable("Appointments");
            modelBuilder.Entity<AppointmentService>().ToTable("AppointmentServices");
            //modelBuilder.Entity<PaymentTransaction>().ToTable("PaymentTransactions");
            modelBuilder.Entity<VaccinationRecord>().ToTable("VaccinationRecords");
        }

        // ==================== Stored Procedure Methods ====================

        /// <summary>
        /// Gọi stored procedure để tạo lịch hẹn mới
        /// Dùng cho luồng thanh toán cọc (từ ConfirmPayment)
        /// </summary>
        public int CreateAppointment(
            int customerId,
            int petId,
            DateTime date,
            string timeSlot,
            string serviceIds,
            string notes,
            bool isDepositPaid,
            string paymentMethod)
        {
            // Khai báo parameters với SqlParameter để tránh SQL injection
            var pCustomerId = new SqlParameter("@CustomerId", customerId);
            var pPetId = new SqlParameter("@PetId", petId);
            var pDate = new SqlParameter("@AppointmentDate", date);
            var pTimeSlot = new SqlParameter("@TimeSlot", timeSlot);
            var pServiceIds = new SqlParameter("@ServiceIds", serviceIds);

            // Xử lý giá trị null cho notes
            var pNotes = new SqlParameter("@CustomerNotes", (object)notes ?? DBNull.Value);

            var pIsDeposit = new SqlParameter("@IsDepositPaid", isDepositPaid);
            var pMethod = new SqlParameter("@PaymentMethod", (object)paymentMethod ?? DBNull.Value);

            // Gọi stored procedure
            // sp_CreateAppointment sẽ return AppointmentId vừa tạo
            var result = this.Database.SqlQuery<int>(
                "EXEC sp_CreateAppointment @CustomerId, @PetId, @AppointmentDate, @TimeSlot, @ServiceIds, @CustomerNotes, @IsDepositPaid, @PaymentMethod",
                pCustomerId, pPetId, pDate, pTimeSlot, pServiceIds, pNotes, pIsDeposit, pMethod
            ).FirstOrDefault();

            return result; // Trả về AppointmentId
        }

        /// <summary>
        /// Lấy chi tiết lịch hẹn (nếu cần dùng từ Context)
        /// </summary>
        public Appointment GetAppointmentById(int appointmentId)
        {
            return this.Appointments
                .Include(a => a.Pet)
                .Include(a => a.Pet.OwnerId)
                .FirstOrDefault(a => a.AppointmentId == appointmentId);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Data.SqlClient;

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

        // Gọi Stored Procedure đặt lịch
        public int CreateAppointment(int customerId, int petId, DateTime date, string timeSlot, string serviceIds, string notes, bool isDepositPaid, string paymentMethod)
        {
            // Khai báo các tham số để truyền vào SQL
            var pCustomerId = new SqlParameter("@CustomerId", customerId);
            var pPetId = new SqlParameter("@PetId", petId);
            var pDate = new SqlParameter("@AppointmentDate", date);
            var pTimeSlot = new SqlParameter("@TimeSlot", timeSlot);
            var pServiceIds = new SqlParameter("@ServiceIds", serviceIds);

            // Xử lý null cho Notes
            var pNotes = new SqlParameter("@CustomerNotes", (object)notes ?? DBNull.Value);

            var pIsDeposit = new SqlParameter("@IsDepositPaid", isDepositPaid);
            var pMethod = new SqlParameter("@PaymentMethod", paymentMethod);

            // Gọi lệnh SQL
            // Database.SqlQuery<int> sẽ trả về AppointmentId vừa tạo (tương ứng SELECT @NewAppointmentId trong SQL)
            var result = this.Database.SqlQuery<int>(
                "sp_CreateAppointment @CustomerId, @PetId, @AppointmentDate, @TimeSlot, @ServiceIds, @CustomerNotes, @IsDepositPaid, @PaymentMethod",
                pCustomerId, pPetId, pDate, pTimeSlot, pServiceIds, pNotes, pIsDeposit, pMethod
            ).FirstOrDefault();

            return result; // Trả về mã lịch hẹn
        }
    }
}
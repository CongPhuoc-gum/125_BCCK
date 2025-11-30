using System;
using System.Linq;
using System.Web.Mvc;
using _125_BCCK.Models;

namespace _125_BCCK.Controllers
{
    /// <summary>
    /// Controller để debug và kiểm tra dữ liệu - XÓA khi deploy production
    /// </summary>
    public class DebugController : Controller
    {
        private PetCareContext db = new PetCareContext();

        // GET: Debug
        public ActionResult Index()
        {
            ViewBag.Message = "Debug Information";
            return View();
        }

        // GET: Debug/CheckDatabase
        public ActionResult CheckDatabase()
        {
            try
            {
                var users = db.Users.ToList();
                var appointments = db.Appointments.ToList();
                var pets = db.Pets.ToList();
                var services = db.Services.ToList();

                ViewBag.UsersCount = users.Count;
                ViewBag.AppointmentsCount = appointments.Count;
                ViewBag.PetsCount = pets.Count;
                ViewBag.ServicesCount = services.Count;

                ViewBag.Users = users;
                ViewBag.Appointments = appointments;
                ViewBag.Pets = pets;

                ViewBag.Success = true;
                ViewBag.Message = "Kết nối database thành công!";
            }
            catch (Exception ex)
            {
                ViewBag.Success = false;
                ViewBag.Message = "Lỗi: " + ex.Message;
                ViewBag.StackTrace = ex.StackTrace;
            }

            return View();
        }

        // GET: Debug/CheckSession
        public ActionResult CheckSession()
        {
            ViewBag.UserId = Session["UserId"];
            ViewBag.FullName = Session["FullName"];
            ViewBag.Email = Session["Email"];
            ViewBag.Role = Session["Role"];

            return View();
        }

        // GET: Debug/Simple
        public ActionResult Simple()
        {
            return View();
        }

        // GET: Debug/UpdateTestData
        public ActionResult UpdateTestData()
        {
            try
            {
                var today = DateTime.Today;
                
                // Update appointments to today and this week
                var apt3 = db.Appointments.Find(3);
                if (apt3 != null) apt3.AppointmentDate = today;

                var apt4 = db.Appointments.Find(4);
                if (apt4 != null) apt4.AppointmentDate = today.AddDays(1);

                var apt1 = db.Appointments.Find(1);
                if (apt1 != null) apt1.AppointmentDate = today.AddDays(-1);

                var apt2 = db.Appointments.Find(2);
                if (apt2 != null) apt2.AppointmentDate = today.AddDays(-2);

                // Assign staff to appointments 5, 6
                var apt5 = db.Appointments.Find(5);
                if (apt5 != null) apt5.StaffId = 2;

                var apt6 = db.Appointments.Find(6);
                if (apt6 != null) apt6.StaffId = 2;

                db.SaveChanges();

                ViewBag.Success = true;
                ViewBag.Message = "Updated test data successfully!";
                ViewBag.Today = today.ToString("dd/MM/yyyy");
            }
            catch (Exception ex)
            {
                ViewBag.Success = false;
                ViewBag.Message = "Error: " + ex.Message;
            }

            return View();
        }

        // GET: Debug/TestStaffData
        public ActionResult TestStaffData()
        {
            try
            {
                // Giả lập session Staff
                Session["UserId"] = 2;
                Session["Role"] = "Staff";

                var staffId = 2;
                var today = DateTime.Today;

                // Lấy tất cả appointments
                var allAppointments = db.Appointments.ToList();
                ViewBag.AllAppointments = allAppointments;

                // Lấy appointments của staff này
                var staffAppointments = db.Appointments
                    .Where(a => a.StaffId == staffId)
                    .ToList();
                ViewBag.StaffAppointments = staffAppointments;

                // Lấy appointments hôm nay
                var todayAppointments = db.Appointments
                    .Where(a => a.StaffId == staffId && 
                               System.Data.Entity.DbFunctions.TruncateTime(a.AppointmentDate) == today)
                    .ToList();
                ViewBag.TodayAppointments = todayAppointments;

                ViewBag.Success = true;
            }
            catch (Exception ex)
            {
                ViewBag.Success = false;
                ViewBag.Message = "Lỗi: " + ex.Message;
                ViewBag.StackTrace = ex.StackTrace;
            }

            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

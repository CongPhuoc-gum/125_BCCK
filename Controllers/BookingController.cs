using _125_BCCK.Helpers;
using _125_BCCK.Models;
using _125_BCCK.Models.ViewModels;
using System;
using System.Linq;
using System.Web.Mvc;

namespace _125_BCCK.Controllers
{
    public class BookingController : Controller
    {
        private PetCareContext db = new PetCareContext();

        // 1. Trang chọn dịch vụ và điền thông tin
        public ActionResult Index()
        {
            if (!SessionHelper.IsLoggedIn())
                return RedirectToAction("Login", "Account");

            // Load danh sách thú cưng của user để hiển thị vào Dropdown
            int userId = SessionHelper.GetUserId().Value;
            ViewBag.Pets = new SelectList(db.Database.SqlQuery<Pet>("SELECT * FROM Pets WHERE OwnerId = " + userId).ToList(), "PetId", "PetName");

            // Load danh sách dịch vụ
            ViewBag.Services = db.Services.Where(s => s.IsActive).ToList();

            return View();
        }

        // 2. Xử lý khi bấm nút "Đặt lịch" -> Chuyển sang trang Thanh toán
        [HttpPost]
        public ActionResult ProcessBooking(BookingViewModel model)
        {
            // Tính tổng tiền dựa trên ServiceIds (vd: "1,2")
            if (string.IsNullOrEmpty(model.ServiceIds)) return View("Index");

            // Tách chuỗi ID để tính tiền (logic đơn giản)
            var ids = model.ServiceIds.Split(',').Select(int.Parse).ToList();
            var services = db.Services.Where(s => ids.Contains(s.ServiceId)).ToList();

            model.TotalAmount = services.Sum(s => s.Price);
            model.DepositAmount = model.TotalAmount * 0.3m; // Tính 30% cọc

            // Chuyển dữ liệu sang trang Thanh toán
            return View("Payment", model);
        }

        // 3. Trang Thanh toán giả lập (Hiển thị mã QR hoặc nút Xác nhận)
        public ActionResult Payment(BookingViewModel model)
        {
            return View(model);
        }

        // 4. Khi user bấm "Thanh toán thành công"
        [HttpPost]
        public ActionResult ConfirmPayment(BookingViewModel model)
        {
            try
            {
                int userId = SessionHelper.GetUserId().Value;

                // Gọi hàm trong Context (đã viết ở Bước 1)
                // Tham số cuối cùng là "Banking" hoặc "QR Code" tuỳ bạn
                db.CreateAppointment(
                    userId,
                    model.PetId,
                    model.AppointmentDate,
                    model.TimeSlot,
                    model.ServiceIds,
                    model.Notes,
                    true, // IsDepositPaid = true (Đã cọc)
                    "Banking"
                );

                return RedirectToAction("BookingSuccess");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Lỗi hệ thống: " + ex.Message;
                return View("Payment", model);
            }
        }

        public ActionResult BookingSuccess()
        {
            return View();
        }
    }
}
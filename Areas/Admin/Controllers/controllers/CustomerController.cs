using _125_BCCK.Models;
using _125_BCCK.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace _125_BCCK.Areas.Admin.Controllers
{
    public class CustomerController : Controller
    {
        private PetCareContext db = new PetCareContext();
        // GET: Admin/Custome
        public ActionResult Index(string searchString)
        {
            var customers = db.Users.Where(u => u.Role == "Customer");
            //Logic tìm kiếm 
            if (!string.IsNullOrEmpty(searchString))
            {
                customers = customers.Where(u => u.FullName.Contains(searchString) || u.Email.Contains(searchString) || u.Phone.Contains(searchString));
            }
            return View(customers.OrderByDescending(u => u.CreatedAt).ToList());    
        }
        //Chi tiết khách hàng 
        public ActionResult Details(int id)
        {
            var user = db.Users.Find(id);
            if (user == null || user.Role != "Customer") return HttpNotFound();

            var viewModel = new CustomerDetailViewModel
            {
                Customer = user,
                //lấy ds thú cưng của khách này 
                Pets = db.Pets.Where(p => p.OwnerId == id).ToList(),
                //Lấy ds lịch hẹn của khách hàng 
                Appointments = db.Appointments.Where(a => a.CustomerId == id).OrderByDescending(a => a.AppointmentDate).ToList()
            };
            return View(viewModel);
        }
        //Sửa thông tin 
        public ActionResult Edit(int id) {
            var user = db.Users.Find(id);
            if (user == null) return HttpNotFound();
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(User user)
        {
            if (ModelState.IsValid)
            {
                var userInDb = db.Users.Find(user.UserId);
                if (userInDb != null)
                {
                    userInDb.FullName = user.FullName;
                    userInDb.Phone = user.Phone;
                    userInDb.Address = user.Address;
                    userInDb.IsActive = user.IsActive; // Cho phép Admin khóa/mở tài khoản
                    userInDb.UpdatedAt = DateTime.Now;

                    db.SaveChanges();
                    TempData["Message"] = "Cập nhật thông tin khách hàng thành công!";
                    return RedirectToAction("Index");
                }
            }
            return View(user);
        }
        //Khóa nhanh 
        public ActionResult ToggleStatus(int id)
        {
            var user = db.Users.Find(id);
            if (user != null)
            {
                user.IsActive = !user.IsActive;
                db.SaveChanges();
                TempData["Message"] = $"Đã thay đổi trạng thái của {user.FullName}";
            }
            return RedirectToAction("Index");
        }

        // 1. GET: Hiển thị trang xác nhận xóa
        public ActionResult Delete(int id)
        {
            var user = db.Users.Find(id);
            if (user == null) return HttpNotFound();
            return View(user);
        }

        // 2. POST: Thực hiện xóa
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var user = db.Users.Find(id);

            // --- KIỂM TRA RÀNG BUỘC DỮ LIỆU ---
            // 1. Kiểm tra xem có Thú cưng không?
            bool hasPets = db.Pets.Any(p => p.OwnerId == id);
            // 2. Kiểm tra xem có Lịch hẹn không?
            bool hasAppointments = db.Appointments.Any(a => a.CustomerId == id);

            if (hasPets || hasAppointments)
            {
                // Nếu dính dữ liệu -> Báo lỗi và không cho xóa
                TempData["Error"] = $"Không thể xóa khách hàng {user.FullName} vì đã có dữ liệu liên quan (Thú cưng hoặc Lịch sử hẹn). Hãy dùng chức năng KHÓA tài khoản!";
                return RedirectToAction("Index");
            }

            // Nếu sạch sẽ -> Xóa luôn
            db.Users.Remove(user);
            db.SaveChanges();
            TempData["Message"] = "Đã xóa khách hàng thành công!";
            return RedirectToAction("Index");
        }
    }
}
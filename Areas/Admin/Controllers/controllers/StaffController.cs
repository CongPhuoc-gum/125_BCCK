using _125_BCCK.Models;
using System;
using System.Collections.Generic;
using System.EnterpriseServices.Internal;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace _125_BCCK.Areas.Admin.Controllers
{
    public class StaffController : Controller
    {
        // GET: Admin/Staff
        // Khởi tạo kết nối csdl
        private PetCareContext db = new PetCareContext();
        //Action index
        public ActionResult Index()
        {
            // lay danh sach nhan vien có role Staff
           var listStaff = db.Users.Where(u => u.Role == "Staff").ToList();
            return View(listStaff);

        }
        //Get: Adimin/Staff/Create(hiện thị form )
        public ActionResult Create()
        {
            return View();
        }
        //Post: Admin/Staff/Create(Xử lý dữ liệu gửi lên )
        [HttpPost]
        [ValidateAntiForgeryToken]//chống hách
        public ActionResult Create(User user)
        {
            if (ModelState.IsValid)
            {
                //Kiểm tra email đẫ tồn tại chưa 
                var checkEmail = db.Users.FirstOrDefault(u => u.Email == user.Email);
                if (checkEmail != null)
                {
                    ModelState.AddModelError("", "Email đã tồn tại");
                    return View(user);
                }
                user.Role = "Staff";
                //Mã hóa mk 
                user.PasswordHash = GetMD5(user.PasswordHash);
                //Lưu thông tin mặc định 
                user.IsActive = true;
                user.CreatedAt = DateTime.Now;
                //Lưu vào database
                db.Users.Add(user);
                db.SaveChanges();
                //Quay ve trang chủ
                return RedirectToAction("Index");
             }    
            //Nếu db sai trả lại form cập nhật lại 
            return View(user);
        }

        //Hàm reset lại mk 
        public ActionResult ResetPassword(int id)
        {
            //tìm nv theo id
            var user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            //Đặt lại mk mặc định 
            user.PasswordHash = GetMD5("123456");//mã hóa trước khi lưu 
            //lưu
            db.SaveChanges();
            TempData["Message"] = $"Đã reset mk cho {user.FullName}";
            return RedirectToAction("Index");
        }

        //Get: Admin/Staff/Edit/{id}
        public ActionResult Edit(int id)
        {
            //Tìm nv theo id
            var user = db.Users.Find(id);
            if (id == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }
        //Post: Admin/Staff/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(User user)
        {
            var userInDb = db.Users.Find(user.UserId);
            if (ModelState.IsValid)
            {
                //Chỉ cập nhật các thông tin cá nhân not pass
                
                userInDb.FullName = user.FullName;
                userInDb.Email = user.Email;
                userInDb.Phone = user.Phone;
                userInDb.Address = user.Address;
                userInDb.IsActive = user.IsActive;
                userInDb.UpdatedAt = DateTime.Now;
                db.SaveChanges();
                TempData["Message"] = "Cập nhật thành công";
                return RedirectToAction("Index");
            }
            return View(user);
        }
        
        //Get : hiển thị trang xác nhận xóa
        public ActionResult Delete(int id)
        {
            var user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }
        //Post: Admin/Staff/Delete Thực hiện xóa khi bấm nút xác nhận xóa
        [HttpPost ,ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            //Tìm nhân viên thheo id
            var user = db.Users.Find(id);
            //kiểm tra xem nv có lịch hẹn nào ko , nếu đẫ có => vô hiệu hóa tk 
            bool hasAppointments = db.Appointments.Any(a => a.StaffId == id);
            if (hasAppointments) {
                TempData["Error"] = "Không thể xóa nhân viên này vì họ đã có lịch sử làm việc. Hãy dùng chức năng 'Khóa tài khoản' thay thế!";
                return RedirectToAction("Index");
            }
            //nếu chưa có gì xóa luôn 
            db.Users.Remove(user);
            db.SaveChanges();
            TempData["Message"] = "Xóa tài khoản thành công";
            return RedirectToAction("Index");
        }

        // Hàm mã hóa MD5 (Dùng tạm cho đồ án sinh viên)
        public static string GetMD5(string str)
        {
            System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(str);
            byte[] hashBytes = md5.ComputeHash(inputBytes);
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString("X2"));
            }
            return sb.ToString();
        }

    }


}


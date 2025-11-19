using _125_BCCK.Helpers;
using _125_BCCK.Models;
using _125_BCCK.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace _125_BCCK.Controllers
{
    public class AccountController : Controller
    {
        private PetCareContext db = new PetCareContext();

        // GET: Account/Login
        public ActionResult Login()
        {
            // Nếu đã đăng nhập, redirect về trang chủ
            if (SessionHelper.IsLoggedIn())
            {
                return RedirectToHome();
            }

            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Hash password
                string hashedPassword = SessionHelper.HashPassword(model.Password);

                // Tìm user
                var user = db.Users.FirstOrDefault(u =>
                    u.Email == model.Email &&
                    u.PasswordHash == hashedPassword &&
                    u.IsActive);

                if (user == null)
                {
                    ModelState.AddModelError("", "Email hoặc mật khẩu không đúng");
                    return View(model);
                }

                // Lưu thông tin vào Session
                SessionHelper.SetUserSession(user.UserId, user.FullName, user.Email, user.Role);

                // Redirect theo Role
                return RedirectToHome();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Đã xảy ra lỗi: " + ex.Message);
                return View(model);
            }
        }

        // GET: Account/Register
        public ActionResult Register()
        {
            // Nếu đã đăng nhập, redirect về trang chủ
            if (SessionHelper.IsLoggedIn())
            {
                return RedirectToHome();
            }

            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Kiểm tra email đã tồn tại chưa
                if (db.Users.Any(u => u.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "Email này đã được đăng ký");
                    return View(model);
                }

                // Tạo user mới
                var user = new User
                {
                    FullName = model.FullName,
                    Email = model.Email,
                    PasswordHash = SessionHelper.HashPassword(model.Password),
                    Phone = model.Phone,
                    Address = model.Address,
                    Role = "Customer",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };

                db.Users.Add(user);
                db.SaveChanges();

                // Tự động đăng nhập sau khi đăng ký
                SessionHelper.SetUserSession(user.UserId, user.FullName, user.Email, user.Role);

                TempData["Success"] = "Đăng ký thành công! Chào mừng bạn đến với PetCare.";
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Đã xảy ra lỗi: " + ex.Message);
                return View(model);
            }
        }

        // GET: Account/Logout
        public ActionResult Logout()
        {
            SessionHelper.ClearSession();
            return RedirectToAction("Index", "Home");
        }

        // Helper: Redirect về trang chủ theo Role
        private ActionResult RedirectToHome()
        {
            string role = SessionHelper.GetRole();

            if (role == "Admin")
                return RedirectToAction("Dashboard", "Admin");
            else if (role == "Staff")
                return RedirectToAction("Dashboard", "Staff");
            else
                // ĐÃ SỬA: Role Customer (hoặc các role khác) sẽ quay về trang chủ
                return RedirectToAction("Index", "Home");
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
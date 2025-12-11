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
                // Kiểm tra email có tồn tại không
                var userByEmail = db.Users.FirstOrDefault(u => u.Email == model.Email);

                if (userByEmail == null)
                {
                    ModelState.AddModelError("", "❌ Email không tồn tại trong hệ thống");
                    return View(model);
                }

                // Kiểm tra IsActive
                if (!userByEmail.IsActive)
                {
                    ModelState.AddModelError("", "❌ Tài khoản đã bị khóa");
                    return View(model);
                }

                // Hash password và TRIM để loại bỏ khoảng trắng thừa
                string hashedPassword = SessionHelper.HashPassword(model.Password).Trim();
                string storedHash = (userByEmail.PasswordHash ?? "").Trim();

                // DEBUG: In ra thông tin
                System.Diagnostics.Debug.WriteLine("=== DEBUG LOGIN ===");
                System.Diagnostics.Debug.WriteLine($"Email: {model.Email}");
                System.Diagnostics.Debug.WriteLine($"Hash generated: {hashedPassword}");
                System.Diagnostics.Debug.WriteLine($"Hash stored:    {storedHash}");
                System.Diagnostics.Debug.WriteLine($"Length generated: {hashedPassword.Length}");
                System.Diagnostics.Debug.WriteLine($"Length stored:    {storedHash.Length}");
                System.Diagnostics.Debug.WriteLine($"Match: {string.Equals(hashedPassword, storedHash, StringComparison.Ordinal)}");

                // Kiểm tra password - DÙNG STRING.EQUALS thay vì ==
                if (!string.Equals(hashedPassword, storedHash, StringComparison.Ordinal))
                {
                    ModelState.AddModelError("", "❌ Mật khẩu không đúng");
                    return View(model);
                }

                // Đăng nhập thành công
                SessionHelper.SetUserSession(userByEmail.UserId, userByEmail.FullName, userByEmail.Email, userByEmail.Role);

                TempData["Success"] = "✅ Đăng nhập thành công!";
                return RedirectToHome();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "❌ LỖI: " + ex.Message);
                System.Diagnostics.Debug.WriteLine($"Exception: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
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
                return RedirectToAction("Index", "homeadmin", new { area = "Admin" });
            else if (role == "Staff")
                return RedirectToAction("Index", "Staff");
            else
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
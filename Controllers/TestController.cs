using System;
using System.Web.Mvc;

namespace _125_BCCK.Controllers
{
    /// <summary>
    /// Controller này chỉ dùng để TEST - XÓA khi deploy production
    /// </summary>
    public class TestController : Controller
    {
        // GET: Test/LoginAsStaff
        // Truy cập: /Test/LoginAsStaff để đăng nhập giả lập với tài khoản Staff
        public ActionResult LoginAsStaff()
        {
            // Giả lập đăng nhập với Staff ID = 2 (Trần Thị Hương)
            Session["UserId"] = 2;
            Session["FullName"] = "Trần Thị Hương";
            Session["Email"] = "huong.staff@petcare.com";
            Session["Role"] = "Staff";

            TempData["Success"] = "Đã đăng nhập giả lập với tài khoản Staff: Trần Thị Hương";
            return RedirectToAction("Index", "Staff");
        }

        // GET: Test/LoginAsStaff2
        // Truy cập: /Test/LoginAsStaff2 để đăng nhập với Staff thứ 2
        public ActionResult LoginAsStaff2()
        {
            // Giả lập đăng nhập với Staff ID = 3 (Lê Văn Cường)
            Session["UserId"] = 3;
            Session["FullName"] = "Lê Văn Cường";
            Session["Email"] = "cuong.staff@petcare.com";
            Session["Role"] = "Staff";

            TempData["Success"] = "Đã đăng nhập giả lập với tài khoản Staff: Lê Văn Cường";
            return RedirectToAction("Index", "Staff");
        }

        // GET: Test/LoginAsCustomer
        // Truy cập: /Test/LoginAsCustomer để đăng nhập giả lập với tài khoản Customer
        public ActionResult LoginAsCustomer()
        {
            // Giả lập đăng nhập với Customer ID = 4 (Phạm Minh Tuấn)
            Session["UserId"] = 4;
            Session["FullName"] = "Phạm Minh Tuấn";
            Session["Email"] = "tuan.customer@gmail.com";
            Session["Role"] = "Customer";

            TempData["Success"] = "Đã đăng nhập giả lập với tài khoản Customer: Phạm Minh Tuấn";
            return RedirectToAction("Index", "Home");
        }

        // GET: Test/LoginAsAdmin
        // Truy cập: /Test/LoginAsAdmin để đăng nhập giả lập với tài khoản Admin
        public ActionResult LoginAsAdmin()
        {
            // Giả lập đăng nhập với Admin ID = 1
            Session["UserId"] = 1;
            Session["FullName"] = "Nguyễn Văn Admin";
            Session["Email"] = "admin@petcare.com";
            Session["Role"] = "Admin";

            TempData["Success"] = "Đã đăng nhập giả lập với tài khoản Admin";
            return RedirectToAction("Index", "Home");
        }

        // GET: Test/Logout
        // Truy cập: /Test/Logout để đăng xuất
        public ActionResult Logout()
        {
            Session.Clear();
            TempData["Success"] = "Đã đăng xuất";
            return RedirectToAction("Index", "Home");
        }

        // GET: Test/Index
        // Trang chính để chọn tài khoản test
        public ActionResult Index()
        {
            return View();
        }
    }
}

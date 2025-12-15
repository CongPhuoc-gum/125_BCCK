using _125_BCCK.Helpers;
using _125_BCCK.Models;
using _125_BCCK.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace _125_BCCK.Controllers
{
    public class BookingController : Controller
    {
        private PetCareContext db = new PetCareContext();

        // ==================== PHẦN ĐẶT LỊCH CHÍNH ====================

        // GET: Trang đặt lịch duy nhất
        public ActionResult Index(int? serviceId = null)
        {
            if (!SessionHelper.IsLoggedIn())
            {
                TempData["Error"] = "Vui lòng đăng nhập để đặt lịch";
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Index", "Booking") });
            }

            int userId = (int)SessionHelper.GetUserId();
            var pets = GetUserPets(userId);

            var viewModel = new BookingViewModel
            {
                Services = GetAllServices(),
                Pets = pets,
                TimeSlots = GetAvailableTimeSlots(),
                PreSelectedServiceId = serviceId,
                NeedAddPet = !pets.Any()
            };

            return View(viewModel);
        }

        // POST: Kiểm tra khung giờ đã đặt (AJAX)
        [HttpPost]
        public JsonResult CheckAvailability(string date)
        {
            try
            {
                DateTime selectedDate = DateTime.Parse(date);
                var bookedSlots = GetBookedTimeSlots(selectedDate);

                return Json(new { success = true, bookedSlots = bookedSlots });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: Tính tổng tiền (AJAX)
        [HttpPost]
        public JsonResult CalculateTotal(string serviceIds)
        {
            try
            {
                if (string.IsNullOrEmpty(serviceIds))
                {
                    return Json(new { success = false, message = "Chưa chọn dịch vụ" });
                }

                var ids = serviceIds.Split(',').Select(int.Parse).ToList();
                var services = GetServicesByIds(ids);

                decimal totalPrice = services.Sum(s => s.Price);
                decimal depositAmount = totalPrice * 0.3m;
                decimal remainingAmount = totalPrice - depositAmount;

                var result = new BookingSummaryViewModel
                {
                    Success = true,
                    TotalPrice = totalPrice,
                    DepositAmount = depositAmount,
                    RemainingAmount = remainingAmount,
                    TotalDuration = services.Sum(s => s.Duration),
                    Services = services.Select(s => new ServiceItemViewModel
                    {
                        ServiceId = s.ServiceId,
                        ServiceName = s.ServiceName,
                        Category = s.Category,
                        Price = s.Price,
                        Duration = s.Duration
                    }).ToList()
                };

                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new BookingSummaryViewModel
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        // ==================== LUỒNG 1: ĐẶT LỊCH TRỰC TIẾP (KHÔNG CỌC) ====================

        // POST: Hoàn tất đặt lịch (thanh toán sau tại cửa hàng)
        [HttpPost]
        public ActionResult CompleteBooking(CreateBookingViewModel model)
        {
            if (!SessionHelper.IsLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Vui lòng điền đầy đủ thông tin";
                return RedirectToAction("Index");
            }

            try
            {
                int customerId = (int)SessionHelper.GetUserId();

                // Nếu chọn thanh toán cọc -> chuyển sang trang Payment
                if (model.PaymentOption == "deposit")
                {
                    return ProcessBooking(model);
                }

                // Nếu chọn thanh toán sau -> tạo lịch hẹn ngay (IsDepositPaid = false)
                using (SqlConnection conn = new SqlConnection(db.Database.Connection.ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("sp_CreateAppointment", conn))
                    {
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@CustomerId", customerId);
                        cmd.Parameters.AddWithValue("@PetId", model.PetId);
                        cmd.Parameters.AddWithValue("@AppointmentDate", model.AppointmentDate);
                        cmd.Parameters.AddWithValue("@TimeSlot", model.TimeSlot);
                        cmd.Parameters.AddWithValue("@ServiceIds", model.ServiceIds);
                        cmd.Parameters.AddWithValue("@CustomerNotes", (object)model.CustomerNotes ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@IsDepositPaid", false); // Chưa cọc
                        cmd.Parameters.AddWithValue("@PaymentMethod", (object)model.PaymentMethod ?? DBNull.Value);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                int appointmentId = Convert.ToInt32(reader["AppointmentId"]);

                                TempData["Success"] = "Đặt lịch thành công! Vui lòng thanh toán tại cửa hàng. Mã lịch hẹn: #" + appointmentId;
                                return RedirectToAction("Success", new { id = appointmentId });
                            }
                        }
                    }
                }

                TempData["Error"] = "Có lỗi xảy ra khi đặt lịch";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // ==================== LUỒNG 2: THANH TOÁN CỌC ====================

        // POST: Xử lý khi chọn "Thanh toán cọc" -> Chuyển sang trang Payment
        [HttpPost]
        public ActionResult ProcessBooking(CreateBookingViewModel model)
        {
            if (!SessionHelper.IsLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            if (string.IsNullOrEmpty(model.ServiceIds))
            {
                TempData["Error"] = "Vui lòng chọn dịch vụ";
                return RedirectToAction("Index");
            }

            try
            {
                // Tính toán số tiền
                var ids = model.ServiceIds.Split(',').Select(int.Parse).ToList();
                var services = GetServicesByIds(ids);

                decimal totalPrice = services.Sum(s => s.Price);
                decimal depositAmount = totalPrice * 0.3m;

                // Tạo ViewModel để hiển thị trang thanh toán
                var paymentModel = new PaymentViewModel
                {
                    PetId = model.PetId,
                    AppointmentDate = model.AppointmentDate,
                    TimeSlot = model.TimeSlot,
                    ServiceIds = model.ServiceIds,
                    CustomerNotes = model.CustomerNotes,
                    PaymentMethod = model.PaymentMethod,
                    TotalPrice = totalPrice,
                    DepositAmount = depositAmount,
                    RemainingAmount = totalPrice - depositAmount,
                    Services = services.Select(s => new ServiceItemViewModel
                    {
                        ServiceId = s.ServiceId,
                        ServiceName = s.ServiceName,
                        Category = s.Category,
                        Price = s.Price,
                        Duration = s.Duration
                    }).ToList()
                };

                return View("Payment", paymentModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // GET: Trang thanh toán (hiển thị QR code, thông tin chuyển khoản)
        public ActionResult Payment(PaymentViewModel model)
        {
            if (!SessionHelper.IsLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            // Kiểm tra nếu model rỗng (truy cập trực tiếp URL)
            if (string.IsNullOrEmpty(model.ServiceIds))
            {
                TempData["Error"] = "Phiên làm việc hết hạn, vui lòng đặt lịch lại";
                return RedirectToAction("Index");
            }

            return View(model);
        }

        // POST: Xác nhận đã thanh toán cọc -> Tạo lịch hẹn 
        [HttpPost]
        public ActionResult ConfirmPayment(PaymentViewModel model, HttpPostedFileBase paymentProofFile)
        {
            if (!SessionHelper.IsLoggedIn())
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập" });
            }

            try
            {
                // Kiểm tra phải có ảnh bill
                if (paymentProofFile == null || paymentProofFile.ContentLength == 0)
                {
                    return Json(new { success = false, message = "Vui lòng tải lên ảnh bill chuyển khoản!" });
                }

                int customerId = (int)SessionHelper.GetUserId();
                string paymentProofPath = null;

                // Upload ảnh bill
                try
                {
                    paymentProofPath = FileUploadHelper.UploadPaymentProof(paymentProofFile, 0); // appointmentId tạm = 0
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = "Lỗi upload ảnh: " + ex.Message });
                }

                using (SqlConnection conn = new SqlConnection(db.Database.Connection.ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("sp_CreateAppointment", conn))
                    {
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@CustomerId", customerId);
                        cmd.Parameters.AddWithValue("@PetId", model.PetId);
                        cmd.Parameters.AddWithValue("@AppointmentDate", model.AppointmentDate);
                        cmd.Parameters.AddWithValue("@TimeSlot", model.TimeSlot);
                        cmd.Parameters.AddWithValue("@ServiceIds", model.ServiceIds);
                        cmd.Parameters.AddWithValue("@CustomerNotes", (object)model.CustomerNotes ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@IsDepositPaid", true); // Đã thanh toán cọc
                        cmd.Parameters.AddWithValue("@PaymentMethod", model.PaymentMethod ?? "BankTransfer");

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                int appointmentId = Convert.ToInt32(reader["AppointmentId"]);

                                // Đổi tên file ảnh theo appointmentId thật
                                if (!string.IsNullOrEmpty(paymentProofPath))
                                {
                                    string oldPath = Server.MapPath(paymentProofPath);
                                    string extension = System.IO.Path.GetExtension(oldPath);
                                    string newFileName = $"payment_{appointmentId}_{DateTime.Now:yyyyMMddHHmmss}{extension}";
                                    string newPath = Server.MapPath($"~/Content/PaymentProofs/{newFileName}");

                                    if (System.IO.File.Exists(oldPath))
                                    {
                                        System.IO.File.Move(oldPath, newPath);
                                        paymentProofPath = $"/Content/PaymentProofs/{newFileName}";
                                    }
                                }

                                // Cập nhật ảnh bill vào PaymentTransactions
                                reader.Close();
                                string updateQuery = @"UPDATE PaymentTransactions 
                                              SET PaymentProofImage = @Image 
                                              WHERE AppointmentId = @AppointmentId 
                                              AND TransactionType = 'Deposit'";
                                using (SqlCommand updateCmd = new SqlCommand(updateQuery, conn))
                                {
                                    updateCmd.Parameters.AddWithValue("@Image", (object)paymentProofPath ?? DBNull.Value);
                                    updateCmd.Parameters.AddWithValue("@AppointmentId", appointmentId);
                                    updateCmd.ExecuteNonQuery();
                                }

                                // Gửi email xác nhận
                                SendBookingConfirmationEmail(appointmentId);

                                return Json(new
                                {
                                    success = true,
                                    message = "Đặt lịch và thanh toán cọc thành công!",
                                    redirectUrl = Url.Action("Success", new { id = appointmentId })
                                });
                            }
                        }
                    }
                }

                return Json(new { success = false, message = "Có lỗi xảy ra khi tạo lịch hẹn" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // Phương thức gửi email xác nhận
        private void SendBookingConfirmationEmail(int appointmentId)
        {
            try
            {
                var appointmentDetails = GetAppointmentDetails(appointmentId);
                if (appointmentDetails == null) return;

                string emailBody = EmailTemplateHelper.GetBookingConfirmationEmail(appointmentDetails);
                string subject = $"[PetCare] Xác nhận đặt lịch #{appointmentId}";

                bool emailSent = EmailHelper.SendMail(appointmentDetails.CustomerEmail, subject, emailBody);

                // Log email
                using (SqlConnection conn = new SqlConnection(db.Database.Connection.ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("sp_LogEmail", conn))
                    {
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@AppointmentId", appointmentId);
                        cmd.Parameters.AddWithValue("@RecipientEmail", appointmentDetails.CustomerEmail);
                        cmd.Parameters.AddWithValue("@EmailType", "BookingConfirmation");
                        cmd.Parameters.AddWithValue("@Subject", subject);
                        cmd.Parameters.AddWithValue("@Body", emailBody);
                        cmd.Parameters.AddWithValue("@IsSuccess", emailSent);
                        cmd.Parameters.AddWithValue("@ErrorMessage", emailSent ? DBNull.Value : (object)"Gửi thất bại");
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Lỗi gửi email: " + ex.Message);
            }
        }

        // ==================== TRANG THÀNH CÔNG ====================

        // GET: Trang thành công
        public ActionResult Success(int id)
        {
            var appointment = GetAppointmentDetails(id);
            if (appointment == null)
            {
                TempData["Error"] = "Không tìm thấy lịch hẹn";
                return RedirectToAction("Index", "Home");
            }

            return View(appointment);
        }

        // Trang thành công đơn giản (dùng cho luồng thanh toán cọc)
        public ActionResult BookingSuccess()
        {
            return View();
        }

        // ==================== HELPER METHODS ====================

        private List<Service> GetAllServices()
        {
            var services = new List<Service>();
            using (SqlConnection conn = new SqlConnection(db.Database.Connection.ConnectionString))
            {
                string query = "SELECT * FROM Services WHERE IsActive = 1 ORDER BY Category, ServiceName";
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    services.Add(new Service
                    {
                        ServiceId = (int)reader["ServiceId"],
                        ServiceName = reader["ServiceName"].ToString(),
                        Description = reader["Description"].ToString(),
                        Category = reader["Category"].ToString(),
                        Duration = (int)reader["Duration"],
                        Price = (decimal)reader["Price"],
                        ImageUrl = reader["ImageUrl"]?.ToString()
                    });
                }
            }
            return services;
        }

        private List<Service> GetServicesByIds(List<int> ids)
        {
            var services = new List<Service>();

            if (ids == null || !ids.Any())
                return services;

            using (SqlConnection conn = new SqlConnection(db.Database.Connection.ConnectionString))
            {
                // Parameterized query để tránh SQL injection
                var parameters = new List<string>();
                var cmd = new SqlCommand();
                cmd.Connection = conn;

                for (int i = 0; i < ids.Count; i++)
                {
                    string paramName = "@id" + i;
                    parameters.Add(paramName);
                    cmd.Parameters.AddWithValue(paramName, ids[i]);
                }

                string query = $"SELECT * FROM Services WHERE ServiceId IN ({string.Join(",", parameters)})";
                cmd.CommandText = query;

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    services.Add(new Service
                    {
                        ServiceId = (int)reader["ServiceId"],
                        ServiceName = reader["ServiceName"].ToString(),
                        Description = reader["Description"].ToString(),
                        Category = reader["Category"].ToString(),
                        Duration = (int)reader["Duration"],
                        Price = (decimal)reader["Price"],
                        ImageUrl = reader["ImageUrl"]?.ToString()
                    });
                }
            }
            return services;
        }

        private List<Pet> GetUserPets(int userId)
        {
            var pets = new List<Pet>();
            using (SqlConnection conn = new SqlConnection(db.Database.Connection.ConnectionString))
            {
                string query = "SELECT * FROM Pets WHERE OwnerId = @UserId AND IsActive = 1";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@UserId", userId);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    pets.Add(new Pet
                    {
                        PetId = (int)reader["PetId"],
                        PetName = reader["PetName"].ToString(),
                        Species = reader["Species"].ToString(),
                        Breed = reader["Breed"]?.ToString(),
                        Age = reader["Age"] != DBNull.Value ? (int?)reader["Age"] : null,
                        Weight = reader["Weight"] != DBNull.Value ? (decimal?)reader["Weight"] : null,
                        Gender = reader["Gender"]?.ToString(),
                        Color = reader["Color"]?.ToString(),
                        ImageUrl = reader["ImageUrl"]?.ToString(),
                        SpecialNotes = reader["SpecialNotes"]?.ToString()
                    });
                }
            }
            return pets;
        }

        private List<string> GetAvailableTimeSlots()
        {
            return new List<string>
            {
                "08:00-09:00", "09:00-10:00", "10:00-11:00", "11:00-12:00",
                "13:00-14:00", "14:00-15:00", "15:00-16:00", "16:00-17:00",
                "17:00-18:00", "18:00-19:00", "19:00-20:00"
            };
        }

        private List<string> GetBookedTimeSlots(DateTime date)
        {
            var bookedSlots = new List<string>();
            using (SqlConnection conn = new SqlConnection(db.Database.Connection.ConnectionString))
            {
                string query = @"SELECT TimeSlot, COUNT(*) as BookingCount 
                                FROM Appointments 
                                WHERE AppointmentDate = @Date AND Status != 'Cancelled'
                                GROUP BY TimeSlot
                                HAVING COUNT(*) >= 3";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Date", date);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    bookedSlots.Add(reader["TimeSlot"].ToString());
                }
            }
            return bookedSlots;
        }

        private BookingSuccessViewModel GetAppointmentDetails(int appointmentId)
        {
            using (SqlConnection conn = new SqlConnection(db.Database.Connection.ConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("sp_GetAppointmentDetails", conn))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@AppointmentId", appointmentId);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var result = new BookingSuccessViewModel
                            {
                                AppointmentId = (int)reader["AppointmentId"],
                                AppointmentDate = (DateTime)reader["AppointmentDate"],
                                TimeSlot = reader["TimeSlot"].ToString(),
                                Status = reader["Status"].ToString(),
                                TotalPrice = (decimal)reader["TotalPrice"],
                                DepositAmount = (decimal)reader["DepositAmount"],
                                DepositPaid = (bool)reader["DepositPaid"],
                                RemainingAmount = (decimal)reader["RemainingAmount"],
                                PetName = reader["PetName"].ToString(),
                                Species = reader["Species"].ToString(),
                                CustomerName = reader["CustomerName"].ToString(),
                                CustomerEmail = reader["CustomerEmail"].ToString(),
                                Services = new List<ServiceItemViewModel>()
                            };

                            // Đọc danh sách dịch vụ (result set thứ 2)
                            if (reader.NextResult())
                            {
                                while (reader.Read())
                                {
                                    result.Services.Add(new ServiceItemViewModel
                                    {
                                        ServiceId = (int)reader["ServiceId"],
                                        ServiceName = reader["ServiceName"].ToString(),
                                        Category = reader["Category"].ToString(),
                                        Price = (decimal)reader["ServicePrice"],
                                        Duration = 0
                                    });
                                }
                            }

                            return result;
                        }
                    }
                }
            }
            return null;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
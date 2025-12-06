using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;
using _125_BCCK.Helpers;
using _125_BCCK.Models;
using _125_BCCK.Models.ViewModels;

namespace _125_BCCK.Controllers
{
    public class BookingController : Controller
    {
        private PetCareContext db = new PetCareContext();

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

        // POST: Hoàn tất đặt lịch
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
                bool isDepositPaid = model.PaymentOption == "deposit";

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
                        cmd.Parameters.AddWithValue("@IsDepositPaid", isDepositPaid);
                        cmd.Parameters.AddWithValue("@PaymentMethod", (object)model.PaymentMethod ?? DBNull.Value);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                int appointmentId = Convert.ToInt32(reader["AppointmentId"]);

                                TempData["Success"] = "Đặt lịch thành công! Mã lịch hẹn: #" + appointmentId;
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

        // ===== HELPER METHODS =====

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

        // FIX 2: Fixed SQL injection vulnerability with parameterized query
        private List<Service> GetServicesByIds(List<int> ids)
        {
            var services = new List<Service>();

            if (ids == null || !ids.Any())
                return services;

            using (SqlConnection conn = new SqlConnection(db.Database.Connection.ConnectionString))
            {
                // Create parameterized query
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
                                        Duration = 0 // Stored procedure không trả về duration
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

        // FIX 3: Add proper disposal
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
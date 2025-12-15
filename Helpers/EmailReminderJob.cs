using System;
using System.Configuration;
using System.Data.SqlClient;
using _125_BCCK.Models.ViewModels;

namespace _125_BCCK.Helpers
{
    public class EmailReminderJob
    {
        /// <summary>
        /// Gửi email nhắc lịch cho các lịch hẹn sắp diễn ra trong 1 giờ tới
        /// </summary>
        public static void SendAppointmentReminders()
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["PetCareDBEntities"].ConnectionString;

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Lấy danh sách lịch hẹn cần nhắc (trong vòng 1-2 giờ tới, chưa gửi email)
                    string query = @"
                        SELECT 
                            a.AppointmentId,
                            a.TimeSlot,
                            c.Email,
                            c.FullName,
                            p.PetName
                        FROM Appointments a
                        INNER JOIN Users c ON a.CustomerId = c.UserId
                        INNER JOIN Pets p ON a.PetId = p.PetId
                        WHERE a.AppointmentDate = CAST(GETDATE() AS DATE)
                            AND a.Status = 'Confirmed'
                            AND a.EmailSent = 0
                            AND DATEDIFF(MINUTE, GETDATE(), 
                                CAST(a.AppointmentDate AS DATETIME) + 
                                CAST(LEFT(a.TimeSlot, 5) AS TIME)) BETWEEN 60 AND 120";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var reminder = new AppointmentReminderViewModel
                                {
                                    AppointmentId = (int)reader["AppointmentId"],
                                    TimeSlot = reader["TimeSlot"].ToString(),
                                    Email = reader["Email"].ToString(),
                                    FullName = reader["FullName"].ToString(),
                                    PetName = reader["PetName"].ToString()
                                };

                                SendReminderEmail(reminder, conn);
                            }
                        }
                    }
                }

                System.Diagnostics.Debug.WriteLine($"[{DateTime.Now}] Email Reminder Job hoàn tất");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[{DateTime.Now}] Lỗi Email Job: {ex.Message}");
            }
        }

        private static void SendReminderEmail(AppointmentReminderViewModel model, SqlConnection conn)
        {
            try
            {
                string emailBody = EmailTemplateHelper.GetAppointmentReminderEmail(model);
                string subject = $"[PetCare] ⏰ Nhắc lịch hẹn #{model.AppointmentId}";

                bool emailSent = EmailHelper.SendMail(model.Email, subject, emailBody);

                if (emailSent)
                {
                    // Cập nhật đã gửi email
                    string updateQuery = @"UPDATE Appointments 
                                          SET EmailSent = 1, EmailSentDate = GETDATE() 
                                          WHERE AppointmentId = @AppointmentId";
                    using (SqlCommand updateCmd = new SqlCommand(updateQuery, conn))
                    {
                        updateCmd.Parameters.AddWithValue("@AppointmentId", model.AppointmentId);
                        updateCmd.ExecuteNonQuery();
                    }

                    // Log email
                    string logQuery = @"INSERT INTO EmailLogs (AppointmentId, RecipientEmail, EmailType, Subject, Body, IsSuccess)
                                       VALUES (@AppointmentId, @Email, 'Reminder', @Subject, @Body, 1)";
                    using (SqlCommand logCmd = new SqlCommand(logQuery, conn))
                    {
                        logCmd.Parameters.AddWithValue("@AppointmentId", model.AppointmentId);
                        logCmd.Parameters.AddWithValue("@Email", model.Email);
                        logCmd.Parameters.AddWithValue("@Subject", subject);
                        logCmd.Parameters.AddWithValue("@Body", emailBody);
                        logCmd.ExecuteNonQuery();
                    }

                    System.Diagnostics.Debug.WriteLine($"✓ Đã gửi email nhắc lịch cho #{model.AppointmentId}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi gửi email #{model.AppointmentId}: {ex.Message}");
            }
        }
    }
}
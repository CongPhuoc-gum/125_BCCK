using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Web;

namespace _125_BCCK.Helpers
{
    public class EmailHelper
    {
        public static bool SendMail(string toEmail, string subject, string body)
        {
            try
            {
                MailMessage mail = new MailMessage();
                mail.To.Add(toEmail);
                mail.From = new MailAddress("phancongphuoc241204@gmail.com", "PetCare System");
                mail.Subject = subject;
                mail.Body = body;
                mail.IsBodyHtml = true;
                mail.BodyEncoding = Encoding.UTF8;

                SmtpClient smtp = new SmtpClient();
                smtp.Send(mail); // Nó sẽ tự đọc user/pass trong Web.config

                return true;
            }
            catch (Exception ex)
            {
                // Để debug xem lỗi gì nếu gửi thất bại
                System.Diagnostics.Debug.WriteLine("Lỗi gửi mail: " + ex.Message);
                return false;
            }
        }
    }
}
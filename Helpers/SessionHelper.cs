using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace _125_BCCK.Helpers
{
    public static class SessionHelper
    {
        public static void SetUserSession(int userId, string fullName, string email, string role)
        {
            HttpContext.Current.Session["UserId"] = userId;
            HttpContext.Current.Session["FullName"] = fullName;
            HttpContext.Current.Session["Email"] = email;
            HttpContext.Current.Session["Role"] = role;
        }

        public static int? GetUserId()
        {
            return HttpContext.Current.Session["UserId"] as int?;
        }

        public static string GetRole()
        {
            return HttpContext.Current.Session["Role"]?.ToString();
        }

        public static string GetFullName()
        {
            return HttpContext.Current.Session["FullName"]?.ToString();
        }

        public static bool IsLoggedIn()
        {
            return HttpContext.Current.Session["UserId"] != null;
        }

        public static void ClearSession()
        {
            HttpContext.Current.Session.Clear();
            HttpContext.Current.Session.Abandon();
        }

        public static string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
using System;
using System.IO;
using System.Linq;
using System.Web;

namespace _125_BCCK.Helpers
{
    public class FileUploadHelper
    {
        private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
        private const int MaxFileSize = 5 * 1024 * 1024; // 5MB

        /// <summary>
        /// Upload ảnh bill chuyển khoản
        /// </summary>
        public static string UploadPaymentProof(HttpPostedFileBase file, int appointmentId)
        {
            try
            {
                // Kiểm tra file có tồn tại không
                if (file == null || file.ContentLength == 0)
                {
                    return null;
                }

                // Kiểm tra kích thước file
                if (file.ContentLength > MaxFileSize)
                {
                    throw new Exception("Kích thước file vượt quá 5MB");
                }

                // Kiểm tra định dạng file
                string fileExtension = Path.GetExtension(file.FileName).ToLower();
                if (!AllowedImageExtensions.Contains(fileExtension))
                {
                    throw new Exception("Chỉ chấp nhận file ảnh: JPG, PNG, GIF, BMP");
                }

                // Tạo tên file unique
                string fileName = $"payment_{appointmentId}_{DateTime.Now:yyyyMMddHHmmss}{fileExtension}";

                // Đường dẫn thư mục lưu file
                string uploadFolder = HttpContext.Current.Server.MapPath("~/Content/PaymentProofs");

                // Tạo thư mục nếu chưa tồn tại
                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }

                // Đường dẫn đầy đủ
                string filePath = Path.Combine(uploadFolder, fileName);

                // Lưu file
                file.SaveAs(filePath);

                // Trả về đường dẫn relative để lưu vào database
                return $"/Content/PaymentProofs/{fileName}";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Lỗi upload file: " + ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Xóa ảnh bill cũ
        /// </summary>
        public static void DeletePaymentProof(string filePath)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath))
                    return;

                string fullPath = HttpContext.Current.Server.MapPath(filePath);

                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Lỗi xóa file: " + ex.Message);
            }
        }
    }
}
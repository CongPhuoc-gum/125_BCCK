using System;
using System.Collections.Generic;
using _125_BCCK.Models.ViewModels;

namespace _125_BCCK.Helpers
{
    public class EmailTemplateHelper
    {
        /// <summary>
        /// Email xác nhận đặt lịch
        /// </summary>
        public static string GetBookingConfirmationEmail(BookingSuccessViewModel model)
        {
            string servicesHtml = "";
            foreach (var service in model.Services)
            {
                servicesHtml += $@"
                    <tr>
                        <td style='padding: 8px; border-bottom: 1px solid #eee;'>{service.ServiceName}</td>
                        <td style='padding: 8px; border-bottom: 1px solid #eee; text-align: right;'>{service.Price:N0}đ</td>
                    </tr>";
            }

            string depositInfo = "";
            if (model.DepositPaid)
            {
                depositInfo = $@"
                    <div style='background: #d4edda; padding: 15px; border-radius: 8px; margin: 15px 0;'>
                        <p style='margin: 0; color: #155724;'>
                            <strong>✓ Đã đặt cọc:</strong> {model.DepositAmount:N0}đ (30%)<br>
                            <strong>Còn lại:</strong> {model.RemainingAmount:N0}đ
                        </p>
                    </div>";
            }
            else
            {
                depositInfo = $@"
                    <div style='background: #fff3cd; padding: 15px; border-radius: 8px; margin: 15px 0;'>
                        <p style='margin: 0; color: #856404;'>
                            <strong>⚠ Chưa thanh toán</strong><br>
                            Vui lòng thanh toán <strong>{model.TotalPrice:N0}đ</strong> khi đến sử dụng dịch vụ
                        </p>
                    </div>";
            }

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; background: #f4f4f4; margin: 0; padding: 20px;'>
    <div style='max-width: 600px; margin: 0 auto; background: white; border-radius: 10px; overflow: hidden; box-shadow: 0 2px 10px rgba(0,0,0,0.1);'>
        
        <!-- Header -->
        <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center;'>
            <h1 style='margin: 0; font-size: 28px;'>🐾 PetCare</h1>
            <p style='margin: 10px 0 0 0; font-size: 16px;'>Xác nhận đặt lịch thành công</p>
        </div>

        <!-- Body -->
        <div style='padding: 30px;'>
            <p style='font-size: 16px;'>Xin chào <strong>{model.CustomerName}</strong>,</p>
            <p>Cảm ơn bạn đã tin tưởng sử dụng dịch vụ của PetCare. Lịch hẹn của bạn đã được xác nhận thành công!</p>

            <!-- Appointment Info -->
            <div style='background: #f8f9fa; padding: 20px; border-radius: 8px; margin: 20px 0;'>
                <h3 style='margin: 0 0 15px 0; color: #667eea;'>📋 Thông tin lịch hẹn</h3>
                <table style='width: 100%; border-collapse: collapse;'>
                    <tr>
                        <td style='padding: 8px 0; font-weight: bold; width: 150px;'>Mã lịch hẹn:</td>
                        <td style='padding: 8px 0; color: #667eea;'><strong>#{model.AppointmentId}</strong></td>
                    </tr>
                    <tr>
                        <td style='padding: 8px 0; font-weight: bold;'>Ngày hẹn:</td>
                        <td style='padding: 8px 0;'>{model.AppointmentDate:dd/MM/yyyy}</td>
                    </tr>
                    <tr>
                        <td style='padding: 8px 0; font-weight: bold;'>Giờ hẹn:</td>
                        <td style='padding: 8px 0;'>{model.TimeSlot}</td>
                    </tr>
                    <tr>
                        <td style='padding: 8px 0; font-weight: bold;'>Thú cưng:</td>
                        <td style='padding: 8px 0;'>{model.PetName} ({model.Species})</td>
                    </tr>
                </table>
            </div>

            <!-- Services -->
            <h3 style='color: #667eea; margin: 20px 0 10px 0;'>📝 Dịch vụ đã chọn</h3>
            <table style='width: 100%; border-collapse: collapse;'>
                {servicesHtml}
                <tr style='background: #f8f9fa;'>
                    <td style='padding: 12px; font-weight: bold; font-size: 16px;'>Tổng tiền:</td>
                    <td style='padding: 12px; text-align: right; font-weight: bold; font-size: 16px; color: #28a745;'>{model.TotalPrice:N0}đ</td>
                </tr>
            </table>

            {depositInfo}

            <!-- Important Notes -->
            <div style='background: #fff3cd; border-left: 4px solid #ffc107; padding: 15px; margin: 20px 0;'>
                <h4 style='margin: 0 0 10px 0; color: #856404;'>⚠️ Lưu ý quan trọng</h4>
                <ul style='margin: 0; padding-left: 20px; color: #856404;'>
                    <li>Vui lòng đến <strong>đúng giờ</strong> theo lịch hẹn</li>
                    <li>Nếu cần hủy/thay đổi, vui lòng thông báo trước <strong>4 tiếng</strong></li>
                    <li>Mang theo thú cưng và giấy tờ liên quan (nếu có)</li>
                </ul>
            </div>

            <p style='text-align: center; margin: 30px 0;'>
                <a href='#' style='display: inline-block; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 12px 30px; text-decoration: none; border-radius: 25px; font-weight: bold;'>
                    Xem chi tiết lịch hẹn
                </a>
            </p>
        </div>

        <!-- Footer -->
        <div style='background: #f8f9fa; padding: 20px; text-align: center; font-size: 14px; color: #6c757d;'>
            <p style='margin: 0 0 10px 0;'><strong>PetCare - Trung tâm chăm sóc thú cưng</strong></p>
            <p style='margin: 0;'>
                📞 Hotline: 1900-xxxx | 📧 Email: support@petcare.com<br>
                📍 Địa chỉ: 123 Nguyễn Huệ, Q1, TP.HCM
            </p>
            <p style='margin: 15px 0 0 0; font-size: 12px;'>
                © 2024 PetCare. All rights reserved.
            </p>
        </div>
    </div>
</body>
</html>";
        }

        /// <summary>
        /// Email nhắc lịch trước 1 giờ
        /// </summary>
        public static string GetAppointmentReminderEmail(AppointmentReminderViewModel model)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; background: #f4f4f4; margin: 0; padding: 20px;'>
    <div style='max-width: 600px; margin: 0 auto; background: white; border-radius: 10px; overflow: hidden; box-shadow: 0 2px 10px rgba(0,0,0,0.1);'>
        
        <!-- Header -->
        <div style='background: linear-gradient(135deg, #ff6b6b 0%, #ffd93d 100%); color: white; padding: 30px; text-align: center;'>
            <h1 style='margin: 0; font-size: 28px;'>⏰ NHẮC LỊCH HẸN</h1>
            <p style='margin: 10px 0 0 0; font-size: 16px;'>Bạn có lịch hẹn sắp tới!</p>
        </div>

        <!-- Body -->
        <div style='padding: 30px;'>
            <p style='font-size: 16px;'>Xin chào <strong>{model.FullName}</strong>,</p>
            <p>Đây là email nhắc nhở về lịch hẹn của bạn tại <strong>PetCare</strong>.</p>

            <div style='background: #fff3cd; border-left: 4px solid #ffc107; padding: 20px; margin: 20px 0; text-align: center;'>
                <h2 style='margin: 0 0 10px 0; color: #856404;'>Lịch hẹn của bạn sẽ bắt đầu sau 1 giờ nữa!</h2>
                <p style='font-size: 24px; font-weight: bold; color: #667eea; margin: 10px 0;'>{model.TimeSlot}</p>
            </div>

            <div style='background: #f8f9fa; padding: 20px; border-radius: 8px; margin: 20px 0;'>
                <table style='width: 100%;'>
                    <tr>
                        <td style='padding: 8px 0; font-weight: bold;'>Mã lịch hẹn:</td>
                        <td style='padding: 8px 0; color: #667eea;'><strong>#{model.AppointmentId}</strong></td>
                    </tr>
                    <tr>
                        <td style='padding: 8px 0; font-weight: bold;'>Thú cưng:</td>
                        <td style='padding: 8px 0;'>{model.PetName}</td>
                    </tr>
                    <tr>
                        <td style='padding: 8px 0; font-weight: bold;'>Khung giờ:</td>
                        <td style='padding: 8px 0;'>{model.TimeSlot}</td>
                    </tr>
                </table>
            </div>

            <div style='background: #d1ecf1; border-left: 4px solid #0c5460; padding: 15px; margin: 20px 0;'>
                <p style='margin: 0; color: #0c5460;'>
                    <strong>💡 Lưu ý:</strong> Vui lòng đến <strong>đúng giờ</strong> để không ảnh hưởng đến lịch trình của bạn và các khách hàng khác.
                </p>
            </div>

            <p style='text-align: center; margin: 30px 0;'>
                <strong>Chúng tôi rất mong được gặp bạn và {model.PetName}! 🐾</strong>
            </p>
        </div>

        <!-- Footer -->
        <div style='background: #f8f9fa; padding: 20px; text-align: center; font-size: 14px; color: #6c757d;'>
            <p style='margin: 0;'>
                📞 Hotline: 1900-xxxx | 📧 Email: support@petcare.com
            </p>
        </div>
    </div>
</body>
</html>";
        }
    }
}
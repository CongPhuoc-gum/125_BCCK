using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace _125_BCCK.Models.ViewModels
{
    public class DashboardViewModel
    {
        //1. các thẻ thống kế
        public decimal DoanhThuNgay { get; set; }
        public decimal DoanhThuThang { get; set; }
        public decimal DoanhThuNam { get; set; }
        public int LichHenChoDuyet { get; set; }
        public int LichHenDangThucHien { get; set; }
        public int LichHenDaHoanThanh { get; set; }
        public int KhachHangMoi { get; set; }

        //2. dữ  liệu biểu đồ doanh thu 
        public List<string> LabelsNgay { get; set; }
        public List<decimal> ValuesDoanhThu { get; set; }

        ///3. Dữ liệu top nhân viên xuất xắc nhất 

        public List<TopStaffViewModel> TopStaff { get; set; }
    }

    public class TopStaffViewModel {
        public string FullName {get; set;}
        public int TotalAppts { get; set;}// số  lich đã thực hiện
        public decimal TotalRevenue {  get; set; }// doanh thu mang về
    }
}
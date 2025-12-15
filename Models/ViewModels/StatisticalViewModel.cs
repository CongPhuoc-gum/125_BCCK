using System.Collections.Generic;

namespace _125_BCCK.Models.ViewModels
{
    public class StatisticalViewModel
    {
        // 1. Số liệu cho các thẻ Card
        public int PendingCount { get; set; }
        public int CompletedCount { get; set; }
        public int TotalCustomers { get; set; }
        public decimal TotalRevenue { get; set; }

        // 2. Dữ liệu cho Biểu đồ Tháng (Line Chart)
        public List<int> MonthlyLabels { get; set; } // Tháng 1, 2...
        public List<decimal> MonthlyRevenue { get; set; } // Doanh thu tương ứng

        // 3. Dữ liệu cho Biểu đồ Năm (Bar Chart)
        public List<int> YearlyLabels { get; set; } // Năm 2023, 2024...
        public List<decimal> YearlyRevenue { get; set; }

        // 4. Dữ liệu Top Dịch vụ (Pie/Doughnut Chart)
        public List<string> ServiceLabels { get; set; }
        public List<decimal> ServiceRevenue { get; set; }

        // 5. Top Khách hàng (Table)
        public List<CustomerStatDTO> TopCustomers { get; set; }
    }

    // Class phụ để hứng dữ liệu khách hàng
    public class CustomerStatDTO
    {
        public string FullName { get; set; }
        public int BookingCount { get; set; }
        public decimal TotalSpent { get; set; }
    }
}
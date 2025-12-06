namespace _125_BCCK.Models.ViewModels
{
    /// <summary>
    /// ViewModel cho từng dịch vụ trong danh sách
    /// </summary>
    public class ServiceItemViewModel
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; }
        public string Category { get; set; }
        public decimal Price { get; set; }
        public int Duration { get; set; }

        // ===== HELPER PROPERTIES =====

        public string CategoryDisplay
        {
            get
            {
                switch (Category)
                {
                    case "Tắm rửa": return "🛁 Tắm rửa";
                    case "Cắt tỉa": return "✂️ Cắt tỉa";
                    case "Y tế": return "💊 Y tế";
                    case "Spa": return "💆 Spa";
                    case "Khác": return "📋 Khác";
                    default: return Category;
                }
            }
        }

        public string CategoryBadgeClass
        {
            get
            {
                switch (Category)
                {
                    case "Tắm rửa": return "badge-primary";
                    case "Cắt tỉa": return "badge-info";
                    case "Y tế": return "badge-danger";
                    case "Spa": return "badge-success";
                    case "Khác": return "badge-secondary";
                    default: return "badge-secondary";
                }
            }
        }

        public string PriceFormatted
        {
            get
            {
                return Price.ToString("N0") + " VNĐ";
            }
        }

        public string DurationDisplay
        {
            get
            {
                if (Duration >= 60)
                {
                    int hours = Duration / 60;
                    int minutes = Duration % 60;

                    if (minutes > 0)
                        return hours + " giờ " + minutes + " phút";
                    else
                        return hours + " giờ";
                }

                return Duration + " phút";
            }
        }
    }
}
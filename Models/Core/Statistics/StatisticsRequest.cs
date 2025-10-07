using System;

namespace CW_RETAIL.Models.Core.Statistics
{
    public class StatisticsRequest
    {
        public string Timeline { get; set; } = "today"; // today, this_week, this_month, this_year, date_range
        public int? StoreId { get; set; } // Optional, only for SuperAdmin
        public DateTime? StartDate { get; set; } // For date_range
        public DateTime? EndDate { get; set; } // For date_range
    }
}
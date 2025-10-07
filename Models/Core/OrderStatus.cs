namespace CW_RETAIL.Models.Core
{
    public static class OrderStatus
    {
        public const int Pending = 0;
        public const int Confirmed = 1;
        public const int Processing = 2;
        public const int AwaitingDelivery = 3;
        public const int Completed = 4;
        public const int Failed = 5;
        public const int Returned = 6;
        public const int Cancelled = 7;

        public static string GetStatusName(int status)
        {
            return status switch
            {
                Pending => "Pending",
                Confirmed => "Confirmed",
                Processing => "Processing",
                AwaitingDelivery => "Awaiting Delivery",
                Completed => "Completed",
                Failed => "Failed",
                Returned => "Returned",
                Cancelled => "Cancelled",
                _ => "Unknown"
            };
        }
    }
}
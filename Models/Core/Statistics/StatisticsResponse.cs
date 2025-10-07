using System;
using System.Collections.Generic;

namespace CW_RETAIL.Models.Core.Statistics
{
    public class StatisticsResponse
    {
        // Order statistics
        public int TotalOrders { get; set; }
        public int TotalStores { get; set; } // For SuperAdmin only
        public int FailedOrders { get; set; }
        public int ConfirmedOrders { get; set; }
        public int ReturnedOrders { get; set; }
        public int DeliveredOrders { get; set; }
        public int TotalOfflineOrders { get; set; } // Orders created by employees

        // Sales statistics
        public decimal TotalSales { get; set; }
        public decimal TotalSalesPrevious { get; set; } // Previous period sales

        // Product statistics
        public int TotalProducts { get; set; }
        public int TotalProductsPrevious { get; set; } // Previous period product count
        public List<CategorySalesItem> TopSellingCategories { get; set; } = new List<CategorySalesItem>();
        public List<ProductSalesItem> TopSellingProducts { get; set; } = new List<ProductSalesItem>();
        public List<ProductSalesItem> LowSellingProducts { get; set; } = new List<ProductSalesItem>();
        public List<CustomerSalesItem> TopCustomers { get; set; } = new List<CustomerSalesItem>();

        // Sales chart data
        public SalesChartData SalesChart { get; set; } = new SalesChartData();
    }

    public class CategorySalesItem
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int TotalSales { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class ProductSalesItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int TotalSales { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class CustomerSalesItem
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class SalesChartData
    {
        public List<string> Labels { get; set; } = new List<string>();
        public List<decimal> Values { get; set; } = new List<decimal>();
    }
}
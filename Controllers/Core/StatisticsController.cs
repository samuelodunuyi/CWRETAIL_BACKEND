using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using CW_RETAIL.Data;
using CW_RETAIL.Models.Core;
using CW_RETAIL.Models.Core.Statistics;

namespace CW_RETAIL.Controllers.Core
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class StatisticsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public StatisticsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<StatisticsResponse>> GetStatistics(
            [FromQuery] string timeline = "today",
            [FromQuery] int? storeId = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            // Get current user and role
            var username = User.Identity?.Name;
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
            {
                return Unauthorized();
            }

            // Determine date range based on timeline
            DateTime calculatedStartDate, calculatedEndDate;
            CalculateDateRange(timeline, startDate, endDate, out calculatedStartDate, out calculatedEndDate);

            // Handle store filtering based on user role
            int? filteredStoreId = null;
            if (user.Role?.Name == "SuperAdmin")
            {
                // SuperAdmin can filter by any store or see all stores
                filteredStoreId = storeId;
            }
           else if (user.Role?.Name == "StoreAdmin" || user.Role?.Name == "Employee")
           {
               // StoreAdmin and Employee can only see their assigned store
               var employee = await _context.Employees.FirstOrDefaultAsync(e => e.UserId == user.Id);
               if (employee != null)
               {
                    filteredStoreId = employee.StoreId;
               }
               else
               {
                   return BadRequest(new { Message = "User is not assigned to any store" });
               }
           }
           else
           {
               return Forbid();
           }

           // Create response object
           var response = new StatisticsResponse
           {
               TopSellingCategories = new List<CategorySalesItem>(),
               TopSellingProducts = new List<ProductSalesItem>(),
               LowSellingProducts = new List<ProductSalesItem>(),
               TopCustomers = new List<CustomerSalesItem>(),
               SalesChart = new SalesChartData
               {
                   Labels = new List<string>(),
                   Values = new List<decimal>()
               }
           };

           // Calculate statistics
            await CalculateOrderStatistics(response, calculatedStartDate, calculatedEndDate, filteredStoreId);
            await CalculateSalesStatistics(response, calculatedStartDate, calculatedEndDate, filteredStoreId);
            await CalculateProductStatistics(response, calculatedStartDate, calculatedEndDate, filteredStoreId);
            await CalculateTopSellingData(response, calculatedStartDate, calculatedEndDate, filteredStoreId);
            await CalculateSalesChartData(response, calculatedStartDate, calculatedEndDate, filteredStoreId, timeline);

           return response;
       }

        private void CalculateDateRange(string timeline, DateTime? customStartDate, DateTime? customEndDate, out DateTime startDate, out DateTime endDate)
        {
            endDate = DateTime.Now;
            
            switch (timeline?.ToLower())
            {
                case "today":
                    startDate = DateTime.Today;
                    break;
                case "this_week":
                    startDate = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
                    break;
                case "this_month":
                    startDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                    break;
                case "this_year":
                    startDate = new DateTime(DateTime.Today.Year, 1, 1);
                    break;
                case "date_range":
                    if (customStartDate.HasValue && customEndDate.HasValue)
                    {
                        startDate = customStartDate.Value;
                        endDate = customEndDate.Value.AddDays(1).AddSeconds(-1); // End of the day
                    }
                    else
                    {
                        startDate = DateTime.Today;
                    }
                    break;
                default:
                    startDate = DateTime.Today;
                    break;
            }
        }

        private async Task CalculateOrderStatistics(StatisticsResponse response, DateTime startDate, DateTime endDate, int? storeId)
        {
            var query = _context.Order.AsQueryable();
            
            // Apply date range filter
            query = query.Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate);
            
            // Apply store filter if specified
            if (storeId.HasValue)
            {
                query = query.Where(o => o.StoreId == storeId.Value);
            }

            // Get total orders
            response.TotalOrders = await query.CountAsync();
            
            // Get orders by status
            response.ConfirmedOrders = await query.CountAsync(o => o.Status == Order.STATUS_CONFIRMED);
            response.FailedOrders = await query.CountAsync(o => o.Status == Order.STATUS_FAILED);
            response.DeliveredOrders = await query.CountAsync(o => o.Status == Order.STATUS_DELIVERED);
            response.ReturnedOrders = await query.CountAsync(o => o.Status == Order.STATUS_RETURNED);
            
            // Get offline orders (created by employees)
            response.TotalOfflineOrders = await query.CountAsync(o => o.CreatedBy != null && o.CreatedBy != "");
            
            // Get total stores (for SuperAdmin only)
            if (!storeId.HasValue)
            {
                response.TotalStores = await _context.Stores.CountAsync(s => s.IsActive);
            }
            else
            {
                response.TotalStores = 1;
            }
        }

        private async Task CalculateSalesStatistics(StatisticsResponse response, DateTime startDate, DateTime endDate, int? storeId)
        {
            var query = _context.Order
                .Include(o => o.OrderItems)
                .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate && o.Status != Order.STATUS_FAILED);
            
            // Apply store filter if specified
            if (storeId.HasValue)
            {
                query = query.Where(o => o.StoreId == storeId.Value);
            }

            // Calculate total sales
            var orders = await query.ToListAsync();
            response.TotalSales = orders.Sum(o => o.OrderItems.Sum(oi => oi.PriceAtOrder * oi.Quantity));
            
            // Calculate previous period sales
            var previousPeriodLength = endDate - startDate;
            var previousPeriodStart = startDate.AddDays(-previousPeriodLength.TotalDays);
            var previousPeriodEnd = startDate.AddSeconds(-1);
            
            var previousQuery = _context.Order
                .Include(o => o.OrderItems)
                .Where(o => o.OrderDate >= previousPeriodStart && o.OrderDate <= previousPeriodEnd && o.Status != Order.STATUS_FAILED);
            
            if (storeId.HasValue)
            {
                previousQuery = previousQuery.Where(o => o.StoreId == storeId.Value);
            }
            
            var previousOrders = await previousQuery.ToListAsync();
            response.TotalSalesPrevious = previousOrders.Sum(o => o.OrderItems.Sum(oi => oi.PriceAtOrder * oi.Quantity));
        }

        private async Task CalculateProductStatistics(StatisticsResponse response, DateTime startDate, DateTime endDate, int? storeId)
        {
            // Get total products
            var productQuery = _context.Products.AsQueryable();
            
            if (storeId.HasValue)
            {
                productQuery = productQuery.Where(p => p.StoreId == storeId.Value);
            }
            
            response.TotalProducts = await productQuery.CountAsync();
            
            // Get previous period product count (products created before the current period)
            var previousProductQuery = _context.Products.Where(p => p.CreatedAt < startDate);
            
            if (storeId.HasValue)
            {
                previousProductQuery = previousProductQuery.Where(p => p.StoreId == storeId.Value);
            }
            
            response.TotalProductsPrevious = await previousProductQuery.CountAsync();
        }

        private async Task CalculateTopSellingData(StatisticsResponse response, DateTime startDate, DateTime endDate, int? storeId)
        {
            // Get orders in the date range
            var query = _context.Order
                .Include(o => o.OrderItems)
                .Include(o => o.Customer)
                .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate && o.Status != Order.STATUS_FAILED);
            
            if (storeId.HasValue)
            {
                query = query.Where(o => o.StoreId == storeId.Value);
            }
            
            var orders = await query.ToListAsync();
            
            // Get products and categories for the order items
            var orderItemIds = orders.SelectMany(o => o.OrderItems.Select(oi => oi.ProductId)).Distinct().ToList();
            var products = await _context.Products
                .Include(p => p.Category)
                .Where(p => orderItemIds.Contains(p.ProductId))
                .ToListAsync();
            
            // Calculate top selling categories
            var categorySales = new List<CategorySalesItem>();
            var categoryGroups = orders
                .SelectMany(o => o.OrderItems)
                .GroupBy(oi => {
                    var product = products.FirstOrDefault(p => p.ProductId == oi.ProductId);
                    return product?.Category;
                })
                .Where(g => g.Key != null)
                .ToList();
                
            foreach (var group in categoryGroups)
            {
                if (group.Key != null)
                {
                    categorySales.Add(new CategorySalesItem
                    {
                        CategoryId = group.Key.CategoryId,
                        CategoryName = group.Key.CategoryName,
                        TotalSales = group.Sum(oi => oi.Quantity),
                        TotalAmount = group.Sum(oi => oi.PriceAtOrder * oi.Quantity)
                    });
                }
            }
            
            response.TopSellingCategories = categorySales
                .OrderByDescending(c => c.TotalSales)
                .Take(5)
                .ToList();
            
            // Calculate top selling products
            var productSales = new List<ProductSalesItem>();
            var productGroups = orders
                .SelectMany(o => o.OrderItems)
                .GroupBy(oi => oi.ProductId)
                .ToList();
                
            foreach (var group in productGroups)
            {
                var product = products.FirstOrDefault(p => p.ProductId == group.Key);
                if (product != null)
                {
                    productSales.Add(new ProductSalesItem
                    {
                        ProductId = product.ProductId,
                        ProductName = product.ProductName,
                        TotalSales = group.Sum(oi => oi.Quantity),
                        TotalAmount = group.Sum(oi => oi.PriceAtOrder * oi.Quantity)
                    });
                }
            }
            
            response.TopSellingProducts = productSales
                .OrderByDescending(p => p.TotalSales)
                .Take(5)
                .ToList();
            
            response.LowSellingProducts = productSales
                .OrderBy(p => p.TotalSales)
                .Take(5)
                .ToList();
            
            // Calculate top customers
            var customerSales = new List<CustomerSalesItem>();
            var customerGroups = orders
                .Where(o => o.Customer != null)
                .GroupBy(o => o.Customer)
                .ToList();
                
            foreach (var group in customerGroups)
            {
                if (group.Key != null)
                {
                    customerSales.Add(new CustomerSalesItem
                    {
                        UserId = group.Key.Id,
                        UserName = group.Key.Username ?? string.Empty,
                        Email = group.Key.Email ?? string.Empty,
                        TotalOrders = group.Count(),
                        TotalAmount = group.Sum(o => o.OrderItems.Sum(oi => oi.PriceAtOrder * oi.Quantity))
                    });
                }
            }
            
            response.TopCustomers = customerSales
                .OrderByDescending(c => c.TotalAmount)
                .Take(5)
                .ToList();
        }

        private async Task CalculateSalesChartData(StatisticsResponse response, DateTime startDate, DateTime endDate, int? storeId, string timeline)
        {
            var query = _context.Order
                .Include(o => o.OrderItems)
                .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate && o.Status != Order.STATUS_FAILED);
            
            if (storeId.HasValue)
            {
                query = query.Where(o => o.StoreId == storeId.Value);
            }
            
            var orders = await query.ToListAsync();
            
            // Initialize sales chart
            response.SalesChart = new SalesChartData
            {
                Labels = new List<string>(),
                Values = new List<decimal>()
            };
            
            if (orders.Count == 0)
            {
                return;
            }
            
            // Group data based on timeline
            switch (timeline?.ToLower())
            {
                case "today":
                    // Group by hour
                    var hourlyData = orders
                        .GroupBy(o => o.OrderDate.Hour)
                        .OrderBy(g => g.Key)
                        .Select(g => new
                        {
                            Label = $"{g.Key}:00",
                            Value = g.Sum(o => o.OrderItems.Sum(oi => oi.PriceAtOrder * oi.Quantity))
                        })
                        .ToList();
                    
                    response.SalesChart.Labels = hourlyData.Select(d => d.Label).ToList();
                    response.SalesChart.Values = hourlyData.Select(d => d.Value).ToList();
                    break;
                
                case "this_week":
                    // Group by day of week
                    var dailyData = orders
                        .GroupBy(o => o.OrderDate.DayOfWeek)
                        .OrderBy(g => g.Key)
                        .Select(g => new
                        {
                            Label = g.Key.ToString(),
                            Value = g.Sum(o => o.OrderItems.Sum(oi => oi.PriceAtOrder * oi.Quantity))
                        })
                        .ToList();
                    
                    response.SalesChart.Labels = dailyData.Select(d => d.Label).ToList();
                    response.SalesChart.Values = dailyData.Select(d => d.Value).ToList();
                    break;
                
                case "this_month":
                    // Group by day of month
                    var monthlyData = orders
                        .GroupBy(o => o.OrderDate.Day)
                        .OrderBy(g => g.Key)
                        .Select(g => new
                        {
                            Label = g.Key.ToString(),
                            Value = g.Sum(o => o.OrderItems.Sum(oi => oi.PriceAtOrder * oi.Quantity))
                        })
                        .ToList();
                    
                    response.SalesChart.Labels = monthlyData.Select(d => d.Label).ToList();
                    response.SalesChart.Values = monthlyData.Select(d => d.Value).ToList();
                    break;
                
                case "this_year":
                    // Group by month
                    var yearlyData = orders
                        .GroupBy(o => o.OrderDate.Month)
                        .OrderBy(g => g.Key)
                        .Select(g => new
                        {
                            Label = new DateTime(2000, g.Key, 1).ToString("MMMM"),
                            Value = g.Sum(o => o.OrderItems.Sum(oi => oi.PriceAtOrder * oi.Quantity))
                        })
                        .ToList();
                    
                    response.SalesChart.Labels = yearlyData.Select(d => d.Label).ToList();
                    response.SalesChart.Values = yearlyData.Select(d => d.Value).ToList();
                    break;
                
                case "date_range":
                    // Group by day if range is less than 60 days, otherwise by week
                    var daysDifference = (endDate - startDate).TotalDays;
                    
                    if (daysDifference <= 60)
                    {
                        var dateRangeData = orders
                            .GroupBy(o => o.OrderDate.Date)
                            .OrderBy(g => g.Key)
                            .Select(g => new
                            {
                                Label = g.Key.ToString("MM/dd"),
                                Value = g.Sum(o => o.OrderItems.Sum(oi => oi.PriceAtOrder * oi.Quantity))
                            })
                            .ToList();
                        
                        response.SalesChart.Labels = dateRangeData.Select(d => d.Label).ToList();
                        response.SalesChart.Values = dateRangeData.Select(d => d.Value).ToList();
                    }
                    else
                    {
                        var weeklyData = orders
                            .GroupBy(o => new { Year = o.OrderDate.Year, Week = GetIso8601WeekOfYear(o.OrderDate) })
                            .OrderBy(g => g.Key.Year)
                            .ThenBy(g => g.Key.Week)
                            .Select(g => new
                            {
                                Label = $"Week {g.Key.Week}",
                                Value = g.Sum(o => o.OrderItems.Sum(oi => oi.PriceAtOrder * oi.Quantity))
                            })
                            .ToList();
                        
                        response.SalesChart.Labels = weeklyData.Select(d => d.Label).ToList();
                        response.SalesChart.Values = weeklyData.Select(d => d.Value).ToList();
                    }
                    break;
                
                default:
                    // Default to daily grouping
                    var defaultData = orders
                        .GroupBy(o => o.OrderDate.Date)
                        .OrderBy(g => g.Key)
                        .Select(g => new
                        {
                            Label = g.Key.ToString("MM/dd"),
                            Value = g.Sum(o => o.OrderItems.Sum(oi => oi.PriceAtOrder * oi.Quantity))
                        })
                        .ToList();
                    
                    response.SalesChart.Labels = defaultData.Select(d => d.Label).ToList();
                    response.SalesChart.Values = defaultData.Select(d => d.Value).ToList();
                    break;
            }
        }

        // Helper method to get ISO 8601 week of year
        private int GetIso8601WeekOfYear(DateTime date)
        {
            var day = (int)System.Globalization.CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(date);
            if (day == 0) day = 7; // Sunday is 7, not 0
            
            // Find Thursday of this week
            var thursday = date.AddDays(4 - day);
            
            // Find the first day of the year that contains this Thursday
            var firstDayOfYear = new DateTime(thursday.Year, 1, 1);
            
            // Calculate the number of days from the first day of the year to the Thursday
            var daysSinceFirstDay = (thursday - firstDayOfYear).TotalDays;
            
            // Calculate the week number
            return (int)(daysSinceFirstDay / 7) + 1;
        }
    }
}
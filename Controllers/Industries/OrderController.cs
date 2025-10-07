using CW_RETAIL.Data;
using CW_RETAIL.Models.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CW_RETAIL.Controllers.Industries
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Order
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            int userRoleValue = int.Parse(userRole);

            // Filter orders based on user role
            if (userRoleValue == UserRole.SuperAdmin)
            {
                // SuperAdmin can see all orders
                return await _context.Order
                    .Include(o => o.OrderItems)
                    .Include(o => o.Store)
                    .ToListAsync();
            }
            else if (userRoleValue == UserRole.StoreAdmin)
            {
                // Get the stores this admin manages
                int userId;
                var storeIds = new List<int>();
                if (int.TryParse(userEmail, out userId))
                {
                    storeIds = await _context.Employees
                        .Where(e => e.UserId == userId)
                        .Select(e => e.StoreId)
                        .ToListAsync();
                }

                // StoreAdmin can see orders for their stores
                return await _context.Order
                    .Include(o => o.OrderItems)
                    .Include(o => o.Store)
                    .Where(o => storeIds.Contains(o.StoreId))
                    .ToListAsync();
            }
            else
            {
                // Regular users (customers) can only see their own orders
                return await _context.Order
                    .Include(o => o.OrderItems)
                    .Include(o => o.Store)
                    .Where(o => o.CreatedBy == userEmail)
                    .ToListAsync();
            }
        }

        // GET: api/Order/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Order>> GetOrder(int id)
        {
            var order = await _context.Order
                .Include(o => o.OrderItems)
                .Include(o => o.Store)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            int userRoleValue = int.Parse(userRole);

            // Check if user has permission to view this order
            if (userRoleValue != UserRole.SuperAdmin)
            {
                if (userRoleValue == UserRole.StoreAdmin)
                {
                    // Check if store admin manages this store
                    int userId;
                    bool managesStore = false;
                    if (int.TryParse(userEmail, out userId))
                    {
                        managesStore = await _context.Employees
                            .AnyAsync(e => e.UserId == userId && e.StoreId == order.StoreId);
                    }

                    if (!managesStore)
                    {
                        return Forbid();
                    }
                }
                else if (order.CreatedBy != userEmail)
                {
                    // Regular users can only view their own orders
                    return Forbid();
                }
            }

            return order;
        }

        // POST: api/Order
        [HttpPost]
        public async Task<ActionResult<Order>> CreateOrder(Order order)
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            
            // Set created by to current user
            order.CreatedBy = userEmail;
            order.LastUpdatedBy = userEmail;
            order.OrderDate = DateTime.UtcNow;
            order.LastUpdatedAt = DateTime.UtcNow;
            order.Status = OrderStatus.Pending;

            // Start a transaction to ensure stock updates are atomic
            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                // Check and update stock for each order item
                foreach (var item in order.OrderItems)
                {
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product == null)
                    {
                        return BadRequest($"Product with ID {item.ProductId} not found");
                    }

                    // Check if enough stock is available
                    if (product.CurrentStock < item.Quantity)
                    {
                        return BadRequest($"Not enough stock for product {product.ProductName}. Available: {product.CurrentStock}, Requested: {item.Quantity}");
                    }

                    // Update product stock
                    product.CurrentStock -= item.Quantity;
                    _context.Entry(product).State = EntityState.Modified;

                    // Set product details in order item
                    item.ProductName = product.ProductName;
                    item.ProductDescription = product.Description;
                    item.ProductCategory = (await _context.Categories.FindAsync(product.CategoryId))?.CategoryName;
                    item.PriceAtOrder = product.BasePrice;
                    item.OriginalPriceAtOrder = product.CostPrice;

                    // Get product image if available
                    var productImage = await _context.ProductImages
                        .Where(pi => pi.ProductId == product.ProductId)
                        .FirstOrDefaultAsync();

                    item.ProductImageUrl = productImage?.ImagePath;
                }

                // Add the order
                _context.Order.Add(order);
                await _context.SaveChangesAsync();

                // Add audit log
                var auditLog = new AuditLog
                {
                    Action = "Create",
                    Details = $"Created new order with {order.OrderItems.Count} items for store {order.StoreId}",
                    Userid = userEmail,
                    CreatedAt = DateTime.UtcNow
                };
                _context.AuditLogs.Add(auditLog);
                await _context.SaveChangesAsync();

                // Commit the transaction
                await transaction.CommitAsync();

                return CreatedAtAction("GetOrder", new { id = order.Id }, order);
            }
            catch (Exception)
            {
                // Rollback the transaction if anything goes wrong
                await transaction.RollbackAsync();
                throw;
            }
        }

        // PUT: api/Order/5/Status
        [HttpPut("{id}/Status")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] int newStatus)
        {
            var order = await _context.Order
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            int userRoleValue = int.Parse(userRole);

            // Check if user has permission to update this order
            if (userRoleValue != UserRole.SuperAdmin)
            {
                if (userRoleValue == UserRole.StoreAdmin)
                {
                    // Check if store admin manages this store
                    int userId;
                    bool managesStore = false;
                    if (int.TryParse(userEmail, out userId))
                    {
                        managesStore = await _context.Employees
                            .AnyAsync(e => e.UserId == userId && e.StoreId == order.StoreId);
                    }

                    if (!managesStore)
                    {
                        return Forbid();
                    }
                }
                else if (order.CreatedBy != userEmail)
                {
                    // Regular users can only update their own orders
                    return Forbid();
                }
            }

            // Check if the new status is valid
            if (newStatus < 0 || newStatus > 6)
            {
                return BadRequest("Invalid order status");
            }
            int statusValue = newStatus;

            // If order is changing to Failed status, restore stock
            if (statusValue == (int)OrderStatus.Failed && order.Status != (int)OrderStatus.Failed)
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                
                try
                {
                    // Restore stock for each order item
                    foreach (var item in order.OrderItems)
                    {
                        var product = await _context.Products.FindAsync(item.ProductId);
                        if (product != null)
                        {
                            // Add the quantity back to stock
                            product.CurrentStock += item.Quantity;
                            _context.Entry(product).State = EntityState.Modified;
                        }
                    }

                    // Update order status
                    order.Status = statusValue;
                    order.LastUpdatedAt = DateTime.UtcNow;
                    order.LastUpdatedBy = userEmail;

                    await _context.SaveChangesAsync();

                    // Add audit log
                    var auditLog = new AuditLog
                {
                    Action = "Update",
                    Details = $"Updated order status to Failed, restored stock for {order.OrderItems.Count} items",
                    Userid = userEmail,
                    CreatedAt = DateTime.UtcNow
                };
                    _context.AuditLogs.Add(auditLog);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            else
            {
                // Just update the status
                order.Status = statusValue;
                order.LastUpdatedAt = DateTime.UtcNow;
                order.LastUpdatedBy = userEmail;

                await _context.SaveChangesAsync();

                // Add audit log
                var auditLog = new AuditLog
                {
                    Action = "Update",
                    Details = $"Order status updated from {GetStatusName(order.Status)} to {GetStatusName(statusValue)}",
                    Userid = userEmail,
                    CreatedAt = DateTime.UtcNow
                };
                _context.AuditLogs.Add(auditLog);
                await _context.SaveChangesAsync();
            }

            return NoContent();
        }

        // Helper method to get status name
        private string GetStatusName(int status)
        {
            return status switch
            {
                OrderStatus.Pending => "Pending",
                OrderStatus.Confirmed => "Confirmed",
                OrderStatus.Processing => "Processing",
                OrderStatus.AwaitingDelivery => "Awaiting Delivery",
                OrderStatus.Completed => "Completed",
                OrderStatus.Failed => "Failed",
                OrderStatus.Returned => "Returned",
                _ => "Unknown"
            };
        }
    }
}
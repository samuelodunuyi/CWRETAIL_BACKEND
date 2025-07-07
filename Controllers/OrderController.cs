
using CWSERVER.Data;
using CWSERVER.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CWSERVER.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrderController(ApiDbContext context) : ControllerBase
    {
        private readonly ApiDbContext _context = context;

        [HttpGet]
        public async Task<IActionResult> GetOrders(
         [FromQuery] int? customerId,
        [FromQuery] int? status,
        [FromQuery] int? storeId,
        [FromQuery] string? createdBy,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate
            
        )
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            IQueryable<Order> query = _context.Orders
                .Include(o => o.Store)
                .Include(o => o.Customer)
                .Include(o => o.OrderItems);

            // Role-based restriction
            if (userRole == "Customer")
            {
                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == userId);
                if (customer == null) return NotFound("Customer not found");
                query = query.Where(o => o.CustomerId == customer.Id);
            }
            else if (userRole == "Employee" || userRole == "StoreRep")
            {
                var employee = await _context.Employees.FirstOrDefaultAsync(e => e.UserId == userId);
                if (employee == null) return NotFound("Employee not found");
                query = query.Where(o => o.StoreId == employee.StoreId);
            }
            else if (userRole == "Admin")
            {
                // Optional filters
                if (customerId.HasValue)
                    query = query.Where(o => o.CustomerId == customerId.Value);

                if (status.HasValue)
                    query = query.Where(o => o.Status == status.Value);

                if (storeId.HasValue)
                    query = query.Where(o => o.StoreId == storeId.Value);

                if (!string.IsNullOrEmpty(createdBy))
                    query = query.Where(o => o.CreatedBy == createdBy);

                if (startDate.HasValue)
                    query = query.Where(o => o.OrderDate >= startDate.Value);

                if (endDate.HasValue)
                    query = query.Where(o => o.OrderDate <= endDate.Value);
            }
           

            var orders = await query.ToListAsync();
            return Ok(orders);
        }


        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] Order orderRequest)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            orderRequest.CreatedBy = userEmail;
            orderRequest.OrderDate = DateTime.UtcNow;

            // Validate Customer
            if (orderRequest.CustomerId.HasValue)
            {
                var customer = await _context.Customers.FindAsync(orderRequest.CustomerId.Value);
                if (customer == null) return BadRequest("Invalid customer");

                if (User.IsInRole("Customer") && customer.UserId != userId)
                    return Forbid();
            }
            else if (User.IsInRole("Customer"))
            {
                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == userId);
                if (customer == null) return BadRequest("No customer profile found for this user.");

                orderRequest.CustomerId = customer.Id;
            }


            // Validate Store permission
            if (User.IsInRole("Employee") || User.IsInRole("StoreRep"))
            {
                var employee = await _context.Employees.FirstOrDefaultAsync(e => e.UserId == userId);
                if (employee == null) return NotFound("Employee not found");
                if (orderRequest.StoreId != employee.StoreId)
                    return Forbid();
            }

            var orderItems = new List<OrderItem>();
            var errors = new List<object>();

            foreach (var item in orderRequest.OrderItems)
            {
                var product = await _context.Products.FindAsync(item.ProductId);

                if (product == null)
                {
                    errors.Add(new { productId = item.ProductId, error = "PRODUCT_NOT_FOUND" });
                    continue;
                }

                if (!product.Status)
                {
                    errors.Add(new { productId = item.ProductId, error = "PRODUCT_INACTIVE" });
                    continue;
                }

                if (product.ProductAmountInStock <= 0)
                {
                    errors.Add(new { productId = product.ProductId, error = "OUT_OF_STOCK" });
                    continue;
                }

                if (item.Quantity > product.ProductAmountInStock)
                {
                    errors.Add(new
                    {
                        productId = product.ProductId,
                        error = "NOT_ENOUGH_STOCK",
                        available = product.ProductAmountInStock
                    });
                    continue;
                }

                // Deduct stock
                product.ProductAmountInStock -= item.Quantity;

                // Create order item
                orderItems.Add(new OrderItem
                {
                    ProductId = product.ProductId,
                    ProductName = product.ProductName,
                    ProductDescription = product.ProductDescription,
                    ProductCategory = product.Category?.CategoryName,
                    ProductImageUrl = product.MainImagePath,
                    PriceAtOrder = product.ProductPrice,
                    OriginalPriceAtOrder = product.ProductOriginalPrice,
                    Quantity = item.Quantity
                });
            }

            if (errors.Any())
                return BadRequest(new { message = "One or more items could not be ordered", errors });

            orderRequest.OrderItems = orderItems;

            _context.Orders.Add(orderRequest);
            await _context.SaveChangesAsync();

            var createdOrder = await _context.Orders
                .Include(o => o.Store)
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == orderRequest.Id);

            return CreatedAtAction(nameof(GetOrderById), new { id = orderRequest.Id }, createdOrder);

            //return CreatedAtAction(nameof(GetOrderById), new { id = orderRequest.Id }, orderRequest);
        }





        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            var order = await _context.Orders
                .Include(o => o.Store)
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

         
            if (userRole == "Customer")
            {
                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == userId);
                if (customer == null || order.CustomerId != customer.Id)
                    return Forbid();
            }
            else if (userRole == "Employee" || userRole == "StoreRep")
            {
                var employee = await _context.Employees.FirstOrDefaultAsync(e => e.UserId == userId);
                if (employee == null || order.StoreId != employee.StoreId)
                    return Forbid();
            }

            return Ok(order);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Employee,StoreRep")]
        public async Task<IActionResult> UpdateOrder(int id, [FromBody] Order updatedOrder)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userEmail = User.FindFirstValue(ClaimTypes.Email);

            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();

          
            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.UserId == userId);
            if (employee == null || order.StoreId != employee.StoreId)
                return Forbid();


            var oldStatus = order.Status;
            order.Status = updatedOrder.Status;
            order.LastUpdatedAt = DateTime.UtcNow;
            order.LastUpdatedBy = userEmail;

            // Restock if order is now canceled/returned/rejected
            var restockStatuses = new[] { 1, 2, 3 };

            if(oldStatus == 5)
            {

                return NoContent();
            }

          

            if (!restockStatuses.Contains(oldStatus) && restockStatuses.Contains(updatedOrder.Status) && oldStatus != 4)
            {
                var orderItems = await _context.OrderItems.Where(oi => oi.OrderId == order.Id).ToListAsync();

                foreach (var item in orderItems)
                {
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product != null)
                    {
                        product.ProductAmountInStock += item.Quantity;
                    }
                }
            }

            

            _context.Orders.Update(order);
            await _context.SaveChangesAsync();

             var updatedOrderWithDetails = await _context.Orders
                .Include(o => o.Store)
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == id);

            return Ok(updatedOrderWithDetails);



            //return NoContent();
        }
    }
}
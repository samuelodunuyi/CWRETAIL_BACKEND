using CWSERVER.Data;
using CWSERVER.Models.Core.Entities;
using CWSERVER.Models.Core.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CWSERVER.Controllers.Core
{
    [Route("api/core/[controller]")]
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
        public async Task<IActionResult> CreateOrder([FromBody] OrderCreateDTO orderDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userEmail = User.FindFirstValue(ClaimTypes.Email);

            // Validate Store exists
            var store = await _context.Stores.FindAsync(orderDto.StoreId);
            if (store == null)
                return BadRequest("Invalid store");

            // Validate Customer
            if (orderDto.CustomerId.HasValue)
            {
                var customer = await _context.Customers.FindAsync(orderDto.CustomerId.Value);
                if (customer == null) return BadRequest("Invalid customer");

                if (User.IsInRole("Customer") && customer.UserId != userId)
                    return Forbid();
            }
            else if (User.IsInRole("Customer"))
            {
                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == userId);
                if (customer == null) return BadRequest("No customer profile found for this user.");

                orderDto.CustomerId = customer.Id;
            }

            // Validate Store permission
            if (User.IsInRole("Employee") || User.IsInRole("StoreRep"))
            {
                var employee = await _context.Employees.FirstOrDefaultAsync(e => e.UserId == userId);
                if (employee == null) return NotFound("Employee not found");
                if (orderDto.StoreId != employee.StoreId)
                    return Forbid();
            }

            var orderItems = new List<OrderItem>();
            var errors = new List<object>();

            foreach (var itemDto in orderDto.OrderItems)
            {
                var product = await _context.Products
                    .Include(p => p.Category)
                    .FirstOrDefaultAsync(p => p.ProductId == itemDto.ProductId);

                if (product == null)
                {
                    errors.Add(new { productId = itemDto.ProductId, error = "PRODUCT_NOT_FOUND" });
                    continue;
                }

                if (!product.Status)
                {
                    errors.Add(new { productId = itemDto.ProductId, error = "PRODUCT_INACTIVE" });
                    continue;
                }

                if (product.ProductAmountInStock <= 0)
                {
                    errors.Add(new { productId = product.ProductId, error = "OUT_OF_STOCK" });
                    continue;
                }

                if (itemDto.Quantity > product.ProductAmountInStock)
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
                product.ProductAmountInStock -= itemDto.Quantity;

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
                    Quantity = itemDto.Quantity,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = userEmail
                });
            }

            if (errors.Any())
                return BadRequest(new { message = "One or more items could not be ordered", errors });

            // Create the order entity
            var order = new Order
            {
                StoreId = orderDto.StoreId,
                CustomerId = orderDto.CustomerId,
                Status = orderDto.Status,
                OrderDate = DateTime.UtcNow,
                CreatedBy = userEmail,
                OrderItems = orderItems
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Return the created order as DTO
            var createdOrder = await _context.Orders
                .Include(o => o.Store)
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == order.Id);

            var responseDto = new OrderResponseDTO
            {
                Id = createdOrder!.Id,
                OrderDate = createdOrder.OrderDate,
                StoreId = createdOrder.StoreId,
                StoreName = createdOrder.Store?.StoreName,
                CustomerId = createdOrder.CustomerId,
                CustomerName = createdOrder.Customer != null ? $"{createdOrder.Customer.FirstName} {createdOrder.Customer.LastName}" : null,
                Status = createdOrder.Status,
                CreatedBy = createdOrder.CreatedBy,
                LastUpdatedAt = createdOrder.LastUpdatedAt,
                LastUpdatedBy = createdOrder.LastUpdatedBy,
                OrderItems = createdOrder.OrderItems.Select(oi => new OrderItemResponseDTO
                {
                    Id = oi.Id,
                    OrderId = oi.OrderId,
                    ProductId = oi.ProductId,
                    ProductName = oi.ProductName,
                    ProductDescription = oi.ProductDescription,
                    ProductCategory = oi.ProductCategory,
                    PriceAtOrder = oi.PriceAtOrder,
                    OriginalPriceAtOrder = oi.OriginalPriceAtOrder,
                    Quantity = oi.Quantity,
                    ProductImageUrl = oi.ProductImageUrl,
                    CreatedAt = oi.CreatedAt,
                    UpdatedAt = oi.UpdatedAt,
                    CreatedBy = oi.CreatedBy,
                    UpdatedBy = oi.UpdatedBy
                }).ToList()
            };

            return CreatedAtAction(nameof(GetOrderById), new { id = order.Id }, responseDto);
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

            var responseDto = new OrderResponseDTO
            {
                Id = order.Id,
                OrderDate = order.OrderDate,
                StoreId = order.StoreId,
                StoreName = order.Store?.StoreName,
                CustomerId = order.CustomerId,
                CustomerName = order.Customer != null ? $"{order.Customer.FirstName} {order.Customer.LastName}" : null,
                Status = order.Status,
                CreatedBy = order.CreatedBy,
                LastUpdatedAt = order.LastUpdatedAt,
                LastUpdatedBy = order.LastUpdatedBy,
                OrderItems = order.OrderItems.Select(oi => new OrderItemResponseDTO
                {
                    Id = oi.Id,
                    OrderId = oi.OrderId,
                    ProductId = oi.ProductId,
                    ProductName = oi.ProductName,
                    ProductDescription = oi.ProductDescription,
                    ProductCategory = oi.ProductCategory,
                    PriceAtOrder = oi.PriceAtOrder,
                    OriginalPriceAtOrder = oi.OriginalPriceAtOrder,
                    Quantity = oi.Quantity,
                    ProductImageUrl = oi.ProductImageUrl,
                    CreatedAt = oi.CreatedAt,
                    UpdatedAt = oi.UpdatedAt,
                    CreatedBy = oi.CreatedBy,
                    UpdatedBy = oi.UpdatedBy
                }).ToList()
            };

            return Ok(responseDto);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Employee,StoreRep")]
        public async Task<IActionResult> UpdateOrder(int id, [FromBody] OrderUpdateDTO orderUpdateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userEmail = User.FindFirstValue(ClaimTypes.Email);

            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();

            // Validate Store exists
            var store = await _context.Stores.FindAsync(orderUpdateDto.StoreId);
            if (store == null)
                return BadRequest("Invalid store");

            // Validate Customer if provided
            if (orderUpdateDto.CustomerId.HasValue)
            {
                var customer = await _context.Customers.FindAsync(orderUpdateDto.CustomerId.Value);
                if (customer == null)
                    return BadRequest("Invalid customer");
            }

            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.UserId == userId);
            if (employee == null || order.StoreId != employee.StoreId)
                return Forbid();

            var oldStatus = order.Status;
            
            // Update order fields
            order.StoreId = orderUpdateDto.StoreId;
            order.CustomerId = orderUpdateDto.CustomerId;
            order.Status = orderUpdateDto.Status;
            order.LastUpdatedAt = DateTime.UtcNow;
            order.LastUpdatedBy = userEmail;

            // Restock if order is now canceled/returned/rejected
            var restockStatuses = new[] { 1, 2, 3 };

            if (oldStatus == 5)
            {
                return BadRequest("Cannot update completed orders");
            }

            if (!restockStatuses.Contains(oldStatus) && restockStatuses.Contains(orderUpdateDto.Status) && oldStatus != 4)
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

            var responseDto = new OrderResponseDTO
            {
                Id = updatedOrderWithDetails!.Id,
                OrderDate = updatedOrderWithDetails.OrderDate,
                StoreId = updatedOrderWithDetails.StoreId,
                StoreName = updatedOrderWithDetails.Store?.StoreName,
                CustomerId = updatedOrderWithDetails.CustomerId,
                CustomerName = updatedOrderWithDetails.Customer != null ? $"{updatedOrderWithDetails.Customer.FirstName} {updatedOrderWithDetails.Customer.LastName}" : null,
                Status = updatedOrderWithDetails.Status,
                CreatedBy = updatedOrderWithDetails.CreatedBy,
                LastUpdatedAt = updatedOrderWithDetails.LastUpdatedAt,
                LastUpdatedBy = updatedOrderWithDetails.LastUpdatedBy,
                OrderItems = updatedOrderWithDetails.OrderItems.Select(oi => new OrderItemResponseDTO
                {
                    Id = oi.Id,
                    OrderId = oi.OrderId,
                    ProductId = oi.ProductId,
                    ProductName = oi.ProductName,
                    ProductDescription = oi.ProductDescription,
                    ProductCategory = oi.ProductCategory,
                    PriceAtOrder = oi.PriceAtOrder,
                    OriginalPriceAtOrder = oi.OriginalPriceAtOrder,
                    Quantity = oi.Quantity,
                    ProductImageUrl = oi.ProductImageUrl,
                    CreatedAt = oi.CreatedAt,
                    UpdatedAt = oi.UpdatedAt,
                    CreatedBy = oi.CreatedBy,
                    UpdatedBy = oi.UpdatedBy
                }).ToList()
            };

            return Ok(responseDto);
        }
    }
}
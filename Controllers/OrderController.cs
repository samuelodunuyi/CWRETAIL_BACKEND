
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
    public class OrderController : ControllerBase
    {
        private readonly ApiDbContext _context;

        public OrderController(ApiDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetOrders()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            IQueryable<Order> query = _context.Orders
                .Include(o => o.Store)
                .Include(o => o.Customer)
                .Include(o => o.OrderItems);

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

            var orders = await query.ToListAsync();
            return Ok(orders);
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] Order order)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userEmail = User.FindFirstValue(ClaimTypes.Email);

          
            order.CreatedBy = userEmail;
            order.OrderDate = DateTime.UtcNow;

           
            var customer = await _context.Customers.FindAsync(order.CustomerId);
            if (customer == null) return BadRequest("Invalid customer");

          
            if (User.IsInRole("Customer") && customer.UserId != userId)
                return Forbid();

            if (User.IsInRole("Employee") || User.IsInRole("StoreRep"))
            {
                var employee = await _context.Employees.FirstOrDefaultAsync(e => e.UserId == userId);
                if (employee == null) return NotFound("Employee not found");

                if (order.StoreId != employee.StoreId)
                    return Forbid();
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetOrderById), new { id = order.Id }, order);
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

       
            order.Status = updatedOrder.Status;
            order.LastUpdatedAt = DateTime.UtcNow;
            order.LastUpdatedBy = userEmail;

            _context.Orders.Update(order);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
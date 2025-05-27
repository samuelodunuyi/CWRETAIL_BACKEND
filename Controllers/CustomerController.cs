using CWSERVER.Data;
using CWSERVER.Models.DTOs;
using CWSERVER.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CWSERVER.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CustomerController : ControllerBase
    {
        private readonly ApiDbContext _context;
        private readonly UserManager<User> _userManager;

        public CustomerController(ApiDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Employee,StoreRep")]
        public async Task<IActionResult> GetAllCustomers(
    [FromQuery] string? email,
    [FromQuery] string? search) 
        {
            var query = _context.Customers.AsQueryable();

           
            if (!string.IsNullOrEmpty(email))
            {
                query = query.Where(c => c.Email == email);
            }

          
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(c =>
                    c.Email.Contains(search) ||
                    c.FirstName.Contains(search) ||
                    c.LastName.Contains(search));
            }

           
            if (User.IsInRole("Employee") || User.IsInRole("StoreRep"))
            {
                var employee = await GetCurrentEmployee();
                if (employee == null) return Forbid();

                query = query.Where(c => c.CreatedBy == employee.User.Email || c.UserId == null);
            }

            var customers = await query.ToListAsync();

           
            var result = customers.Select(c => new CustomerDto
            {
                Id = c.Id,
                UserId = c.UserId,
                FirstName = c.FirstName,
                LastName = c.LastName,
                Email = c.Email,
                PhoneNumber = c.PhoneNumber,
                CreatedBy = c.CreatedBy,
                CreatedAt = c.CreatedAt
            });

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCustomerById(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null) return NotFound();

            
            var result = new CustomerDto
            {
                Id = customer.Id,
                UserId = customer.UserId,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Email = customer.Email,
                PhoneNumber = customer.PhoneNumber,
                CreatedBy = customer.CreatedBy,
                CreatedAt = customer.CreatedAt
            };

            return Ok(result);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> CreateCustomer([FromBody] Customer customer)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

           
            if (User.Identity.IsAuthenticated)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                customer.CreatedBy = currentUser?.Email;
            }

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCustomerById), new { id = customer.Id }, customer);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCustomer(int id, [FromBody] Customer updatedCustomer)
        {
            if (id != updatedCustomer.Id)
                return BadRequest("ID mismatch");

            var existingCustomer = await _context.Customers.FindAsync(id);
            if (existingCustomer == null) return NotFound();

           
            if (User.IsInRole("Customer"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (existingCustomer.UserId != userId)
                    return Forbid();
            }
            else if (User.IsInRole("Employee") || User.IsInRole("StoreRep"))
            {
                var employee = await GetCurrentEmployee();
                if (employee == null || (existingCustomer.CreatedBy != employee.User.Email && existingCustomer.UserId != null))
                    return Forbid();
            }

            _context.Entry(existingCustomer).CurrentValues.SetValues(updatedCustomer);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null) return NotFound();

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private async Task<Employee> GetCurrentEmployee()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return await _context.Employees
                .Include(e => e.User)
                .FirstOrDefaultAsync(e => e.UserId == userId);
        }
    }
}
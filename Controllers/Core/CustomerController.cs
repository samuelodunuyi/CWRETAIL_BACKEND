using CWSERVER.Data;
using CWSERVER.Models.Core.Entities;
using CWSERVER.Models.Core.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CWSERVER.Controllers.Core
{
    [Route("api/core/[controller]")]
    [ApiController]
    [Authorize]
    public class CustomerController(ApiDbContext context, UserManager<User> userManager) : ControllerBase
    {
        private readonly ApiDbContext _context = context;
        private readonly UserManager<User> _userManager = userManager;

        [HttpGet]
        [Authorize(Roles = "Admin,Employee,StoreRep")]
        public async Task<IActionResult> GetAllCustomers([FromQuery] string? email, [FromQuery] string? search) 
        {
            var query = _context.Customers.AsQueryable();

           
            if (!string.IsNullOrEmpty(email))
            {
                query = query.Where(c => c.Email == email);
            }

          
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(c =>
                    c.Email!.Contains(search) ||
                    c.FirstName!.Contains(search) ||
                    c.LastName!.Contains(search));
            }

           
            if (User.IsInRole("Employee") || User.IsInRole("StoreRep"))
            {
                var employee = await GetCurrentEmployee();
                if (employee == null) return Forbid();

                query = query.Where(c => c.CreatedBy == employee.User!.Email || c.UserId == null);
            }

            var customers = await query.ToListAsync();

           
            var result = customers.Select(c => new CustomerResponseDTO
            {
                Id = c.Id,
                UserId = c.UserId,
                FirstName = c.FirstName,
                LastName = c.LastName,
                Email = c.Email,
                PhoneNumber = c.PhoneNumber,
                CreatedBy = c.CreatedBy,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                UpdatedBy = c.UpdatedBy
            });

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCustomerById(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null) return NotFound();

            
            var result = new CustomerResponseDTO
            {
                Id = customer.Id,
                UserId = customer.UserId,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Email = customer.Email,
                PhoneNumber = customer.PhoneNumber,
                CreatedBy = customer.CreatedBy,
                CreatedAt = customer.CreatedAt,
                UpdatedAt = customer.UpdatedAt,
                UpdatedBy = customer.UpdatedBy
            };

            return Ok(result);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> CreateCustomer([FromBody] CustomerCreateDTO customerDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Validate foreign key reference if UserId is provided
            if (customerDto.UserId != null)
            {
                var userExists = await _userManager.FindByIdAsync(customerDto.UserId);
                if (userExists == null)
                    return BadRequest($"User with ID {customerDto.UserId} does not exist.");
            }

            var customer = new Customer
            {
                UserId = customerDto.UserId,
                FirstName = customerDto.FirstName,
                LastName = customerDto.LastName,
                Email = customerDto.Email,
                PhoneNumber = customerDto.PhoneNumber,
                CreatedAt = DateTime.UtcNow
            };

            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                customer.CreatedBy = currentUser?.Email;
            }
            else
            {
                customer.CreatedBy = "System";
            }

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            var result = new CustomerResponseDTO
            {
                Id = customer.Id,
                UserId = customer.UserId,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Email = customer.Email,
                PhoneNumber = customer.PhoneNumber,
                CreatedBy = customer.CreatedBy,
                CreatedAt = customer.CreatedAt,
                UpdatedAt = customer.UpdatedAt,
                UpdatedBy = customer.UpdatedBy
            };

            return CreatedAtAction(nameof(GetCustomerById), new { id = customer.Id }, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCustomer(int id, [FromBody] CustomerUpdateDTO customerDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingCustomer = await _context.Customers.FindAsync(id);
            if (existingCustomer == null) return NotFound();

            // Validate foreign key reference if UserId is provided
            if (customerDto.UserId != null)
            {
                var userExists = await _userManager.FindByIdAsync(customerDto.UserId);
                if (userExists == null)
                    return BadRequest($"User with ID {customerDto.UserId} does not exist.");
            }

           
            if (User.IsInRole("Customer"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (existingCustomer.UserId != userId)
                    return Forbid();
            }
            else if (User.IsInRole("Employee") || User.IsInRole("StoreRep"))
            {
                var employee = await GetCurrentEmployee();
                if (employee == null || existingCustomer.CreatedBy != employee.User!.Email && existingCustomer.UserId != null)
                    return Forbid();
            }

            existingCustomer.UserId = customerDto.UserId;
            existingCustomer.FirstName = customerDto.FirstName;
            existingCustomer.LastName = customerDto.LastName;
            existingCustomer.Email = customerDto.Email;
            existingCustomer.PhoneNumber = customerDto.PhoneNumber;
            existingCustomer.UpdatedAt = DateTime.UtcNow;

            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                existingCustomer.UpdatedBy = currentUser?.Email;
            }
            else
            {
                existingCustomer.UpdatedBy = "System";
            }

            await _context.SaveChangesAsync();

            var result = new CustomerResponseDTO
            {
                Id = existingCustomer.Id,
                UserId = existingCustomer.UserId,
                FirstName = existingCustomer.FirstName,
                LastName = existingCustomer.LastName,
                Email = existingCustomer.Email,
                PhoneNumber = existingCustomer.PhoneNumber,
                CreatedBy = existingCustomer.CreatedBy,
                CreatedAt = existingCustomer.CreatedAt,
                UpdatedAt = existingCustomer.UpdatedAt,
                UpdatedBy = existingCustomer.UpdatedBy
            };

            return Ok(result);
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

        private async Task<Employee?> GetCurrentEmployee()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return null; 

            return await _context.Employees
                .Include(e => e.User)
                .FirstOrDefaultAsync(e => e.UserId == userId);
        }
    }
}
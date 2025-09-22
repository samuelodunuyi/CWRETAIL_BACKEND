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
    [Authorize(Roles = "Admin,StoreRep")]
    public class EmployeeController(ApiDbContext context, UserManager<User> userManager) : ControllerBase
    {
        private readonly ApiDbContext _context = context;
        private readonly UserManager<User> _userManager = userManager;

        [HttpGet]
        public async Task<IActionResult> GetAllEmployees([FromQuery] int? storeId, [FromQuery] string? email)
        {
            var query = _context.Employees
                .Include(e => e.User)
                .Include(e => e.Store)
                .AsQueryable();

            if (storeId.HasValue && User.IsInRole("Admin"))
            {
                query = query.Where(e => e.StoreId == storeId.Value);
            }
            else if (User.IsInRole("StoreRep"))
            {
                var currentEmployee = await GetCurrentEmployee();
                if (currentEmployee == null) return Forbid();

                query = query.Where(e => e.StoreId == currentEmployee.StoreId);
            }

            if (!string.IsNullOrEmpty(email))
            {
                query = query.Where(e => e.User != null && e.User.Email != null && e.User.Email.Contains(email));
            }

            var employees = await query.ToListAsync();

            var result = employees.Select(e => new EmployeeResponseDTO
            {
                Id = e.Id,
                UserId = e.UserId,
                StoreId = e.StoreId,
                StoreName = e.Store?.StoreName,
                PhoneNumber = e.PhoneNumber,
                FirstName = e.FirstName,
                LastName = e.LastName,
                CreatedAt = e.CreatedAt,
                UpdatedAt = e.UpdatedAt,
                UpdatedBy = e.UpdatedBy
            });

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetEmployeeById(int id)
        {
            var employee = await _context.Employees
                .Include(e => e.User)
                .Include(e => e.Store)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (employee == null) return NotFound();

        
            if (User.IsInRole("StoreRep"))
            {
                var currentEmployee = await GetCurrentEmployee();
                if (currentEmployee == null || employee.StoreId != currentEmployee.StoreId)
                    return Forbid();
            }

          
            var result = new EmployeeResponseDTO
            {
                Id = employee.Id,
                UserId = employee.UserId,
                StoreId = employee.StoreId,
                StoreName = employee.Store?.StoreName,
                PhoneNumber = employee.PhoneNumber,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                CreatedAt = employee.CreatedAt,
                UpdatedAt = employee.UpdatedAt,
                UpdatedBy = employee.UpdatedBy
            };

            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateEmployee([FromBody] EmployeeCreateDTO employeeDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Validate foreign key references
            var user = await _userManager.FindByIdAsync(employeeDto.UserId!);
            if (user == null) return BadRequest($"User with ID {employeeDto.UserId} does not exist.");

            var storeExists = await _context.Stores.AnyAsync(s => s.StoreId == employeeDto.StoreId);
            if (!storeExists)
                return BadRequest($"Store with ID {employeeDto.StoreId} does not exist.");

            var employee = new Employee
            {
                UserId = employeeDto.UserId,
                StoreId = employeeDto.StoreId,
                PhoneNumber = employeeDto.PhoneNumber,
                FirstName = employeeDto.FirstName,
                LastName = employeeDto.LastName,
                CreatedAt = DateTime.UtcNow
            };

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            // Load the store for response
            await _context.Entry(employee).Reference(e => e.Store).LoadAsync();

            var result = new EmployeeResponseDTO
            {
                Id = employee.Id,
                UserId = employee.UserId,
                StoreId = employee.StoreId,
                StoreName = employee.Store?.StoreName,
                PhoneNumber = employee.PhoneNumber,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                CreatedAt = employee.CreatedAt,
                UpdatedAt = employee.UpdatedAt,
                UpdatedBy = employee.UpdatedBy
            };

            return CreatedAtAction(nameof(GetEmployeeById), new { id = employee.Id }, result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> UpdateEmployee(int id, [FromBody] EmployeeUpdateDTO employeeDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var currentEmployee = await GetCurrentEmployee();
            if (currentEmployee == null)
                return Unauthorized("Employee not found");

            var employee = await _context.Employees.Include(e => e.Store).FirstOrDefaultAsync(e => e.Id == id);
            if (employee == null)
                return NotFound();

            // Managers can only update employees in their own store
            if (currentEmployee.User!.Role == "Manager" && employee.StoreId != currentEmployee.StoreId)
                return Forbid("You can only update employees in your own store");

            // Validate foreign key references if they are being updated
            if (!string.IsNullOrEmpty(employeeDto.UserId))
            {
                var user = await _userManager.FindByIdAsync(employeeDto.UserId);
                if (user == null) return BadRequest($"User with ID {employeeDto.UserId} does not exist.");
                employee.UserId = employeeDto.UserId;
            }

            if (employeeDto.StoreId.HasValue)
            {
                var storeExists = await _context.Stores.AnyAsync(s => s.StoreId == employeeDto.StoreId.Value);
                if (!storeExists)
                    return BadRequest($"Store with ID {employeeDto.StoreId} does not exist.");
                employee.StoreId = employeeDto.StoreId.Value;
            }

            // Update other fields
            if (!string.IsNullOrEmpty(employeeDto.PhoneNumber))
                employee.PhoneNumber = employeeDto.PhoneNumber;
            if (!string.IsNullOrEmpty(employeeDto.FirstName))
                employee.FirstName = employeeDto.FirstName;
            if (!string.IsNullOrEmpty(employeeDto.LastName))
                employee.LastName = employeeDto.LastName;

            employee.UpdatedAt = DateTime.UtcNow;
            employee.UpdatedBy = "System"; // TODO: Get from authenticated user

            await _context.SaveChangesAsync();

            var result = new EmployeeResponseDTO
            {
                Id = employee.Id,
                UserId = employee.UserId,
                StoreId = employee.StoreId,
                StoreName = employee.Store?.StoreName,
                PhoneNumber = employee.PhoneNumber,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                CreatedAt = employee.CreatedAt,
                UpdatedAt = employee.UpdatedAt,
                UpdatedBy = employee.UpdatedBy
            };

            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null) return NotFound();

            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private async Task<Employee?> GetCurrentEmployee()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is null) return null;

            return await _context.Employees.FirstOrDefaultAsync(e => e.UserId == userId);
        }
    }
}
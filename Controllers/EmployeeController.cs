using CWSERVER.Data;
using CWSERVER.Models.Core.Entities;
using CWSERVER.Models.Core.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CWSERVER.Controllers
{
    [Route("api/[controller]")]
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

            var result = employees.Select(e => new EmployeeDto
            {
                Id = e.Id,
                UserId = e.UserId,
                Role = e.User?.Role,
                IsActive = e.User?.IsActive ?? false,
                CreatedAt = e.User?.CreatedAt ?? DateTime.MinValue,
                LastUpdatedAt = e.User?.LastUpdatedAt ?? DateTime.MinValue,
                LastUpdatedBy = e.User?.LastUpdatedBy,
                Email = e.User?.Email,
                PhoneNumber = e.PhoneNumber,
                FirstName = e.FirstName,
                LastName = e.LastName,
                Store = e.Store != null ? new StoreDto
                {
                    StoreId = e.Store.StoreId,
                    StoreName = e.Store.StoreName,
                    StoreRep = e.Store.StoreRep
                } : null
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

          
            var result = new EmployeeDto
            {
                Id = employee.Id,
                UserId = employee.UserId,
                Role = employee.User?.Role,
                IsActive = employee.User!.IsActive,
                CreatedAt = employee.User.CreatedAt,
                LastUpdatedAt = employee.User.LastUpdatedAt,
                LastUpdatedBy = employee.User.LastUpdatedBy,
                Email = employee.User.Email,
                PhoneNumber = employee.PhoneNumber,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                Store = new StoreDto
                {
                    StoreId = employee.Store!.StoreId,
                    StoreName = employee.Store.StoreName,
                    StoreRep = employee.Store.StoreRep
                }
            };

            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateEmployee([FromBody] Employee employee)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

        
            var user = await _userManager.FindByIdAsync(employee.UserId!);
            if (user == null) return BadRequest("User not found");

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEmployeeById), new { id = employee.Id }, employee);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEmployee(int id, [FromBody] Employee updatedEmployee)
        {
            if (id != updatedEmployee.Id)
                return BadRequest("ID mismatch");

            var existingEmployee = await _context.Employees.FindAsync(id);
            if (existingEmployee == null) return NotFound();

         
            if (User.IsInRole("StoreRep"))
            {
                var currentEmployee = await GetCurrentEmployee();
                if (currentEmployee == null || existingEmployee.StoreId != currentEmployee.StoreId)
                    return Forbid();
            }

            _context.Entry(existingEmployee).CurrentValues.SetValues(updatedEmployee);
            await _context.SaveChangesAsync();

            return NoContent();
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
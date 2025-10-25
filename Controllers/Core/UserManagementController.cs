using CW_RETAIL.Data;
using CW_RETAIL.Models.Auth;
using CW_RETAIL.Models.Core;
using CW_RETAIL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CW_RETAIL.Models.Core
{
    public class StoreAdminAssignmentRequest
    {
        public string StoreId { get; set; }
        public string UserEmail { get; set; }
    }
}

namespace CW_RETAIL.Controllers.Core
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserManagementController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly ApplicationDbContext _context;

        public UserManagementController(AuthService authService, ApplicationDbContext context)
        {
            _authService = authService;
            _context = context;
        }

        // Only SuperAdmin can access this endpoint
        [HttpPost("create-store-admin")]
        public async Task<IActionResult> CreateStoreAdmin([FromBody] RegisterRequest model)
        {
            // Check if user is SuperAdmin
            if (!User.IsInRole("SuperAdmin"))
            {
                return Forbid();
            }

            // Set role to StoreAdmin
            model.RoleId = UserRole.StoreAdmin;

            var result = await _authService.RegisterAsync(
                model.Username, 
                model.Email, 
                model.Password, 
                model.RoleId,
                model.FirstName,
                model.LastName);

            if (!result)
            {
                return BadRequest(new { Message = "User already exists" });
            }

            // Log the action
            await LogAuditAsync("Create StoreAdmin", $"Created StoreAdmin: {model.Email}");

            return Ok(new { Message = "Store Admin created successfully" });
        }

        // Only SuperAdmin can access this endpoint
        [HttpPost("create-employee")]
        public async Task<IActionResult> CreateEmployee([FromBody] RegisterRequest model)
        {
            // Check if user is SuperAdmin
            if (!User.IsInRole("SuperAdmin"))
            {
                return Forbid();
            }

            // Set role to Employee
            model.RoleId = UserRole.Employee;

            var result = await _authService.RegisterAsync(
                model.Username, 
                model.Email, 
                model.Password, 
                model.RoleId,
                model.FirstName,
                model.LastName);

            if (!result)
            {
                return BadRequest(new { Message = "User already exists" });
            }

            // Log the action
            await LogAuditAsync("Create Employee", $"Created Employee: {model.Email}");

            return Ok(new { Message = "Employee created successfully" });
        }
        
        // Only SuperAdmin can access this endpoint
        [HttpPut("set-user-status")]
        public async Task<IActionResult> SetUserStatus(int userId, bool isActive)
        {
            // Check if user is SuperAdmin
            if (!User.IsInRole("SuperAdmin"))
            {
                return Forbid();
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return NotFound(new { Message = "User not found" });
            }

            // Check if user is not a SuperAdmin (SuperAdmin can't modify other SuperAdmins)
            if (user.RoleId == UserRole.SuperAdmin)
            {
                return BadRequest(new { Message = "Cannot modify SuperAdmin status" });
            }

            user.IsActive = isActive;
            await _context.SaveChangesAsync();

            // Log the action
            string action = isActive ? "Activate User" : "Deactivate User";
            await LogAuditAsync(action, $"{action}: {user.Email}");

            return Ok(new { Message = $"User status updated successfully. User is now {(isActive ? "active" : "inactive")}" });
        }

        [HttpPost("assign-storeadmin")]
        public async Task<IActionResult> AssignStoreAdmin([FromBody] StoreAdminAssignmentRequest request)
        {
            // Check if user is SuperAdmin
            if (!User.IsInRole("SuperAdmin"))
            {
                return Forbid();
            }

            var store = await _context.Stores.FindAsync(int.Parse(request.StoreId));
            if (store == null)
            {
                return NotFound(new { Message = "Store not found" });
            }

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == request.UserEmail);

            if (user == null)
            {
                return NotFound(new { Message = "User not found" });
            }

            if (user.RoleId != UserRole.StoreAdmin)
            {
                return BadRequest(new { Message = "User is not a Store Admin" });
            }

            store.StoreAdmin = request.UserEmail;
            store.UserId = user.Id;
            store.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Log the action
            await LogAuditAsync("Assign StoreAdmin", $"Assigned StoreAdmin {request.UserEmail} to Store {store.StoreName}");

            return Ok(new { Message = "Store Admin assigned successfully" });
        }

        [HttpPost("assign-employee")]
        public async Task<IActionResult> AssignEmployee(int storeId, string userEmail)
        {
            // Check if user is SuperAdmin or StoreAdmin
            if (!User.IsInRole("SuperAdmin") && !User.IsInRole("StoreAdmin"))
            {
                return Forbid();
            }

            var store = await _context.Stores.FindAsync(storeId);
            if (store == null)
            {
                return NotFound(new { Message = "Store not found" });
            }

            // If StoreAdmin, check if they are assigned to this store
            if (User.IsInRole("StoreAdmin"))
            {
                var currentUserEmail = User.FindFirstValue(ClaimTypes.Email);
                if (store.StoreAdmin != currentUserEmail)
                {
                    return Forbid();
                }
            }

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == userEmail);

            if (user == null)
            {
                return NotFound(new { Message = "User not found" });
            }

            if (user.RoleId != UserRole.Employee)
            {
                return BadRequest(new { Message = "User is not an Employee" });
            }

            // Check if employee is already assigned to a store
            var existingEmployee = await _context.Employees
                .FirstOrDefaultAsync(e => e.UserId == user.Id);

            if (existingEmployee != null)
            {
                existingEmployee.StoreId = storeId;
            }
            else
            {
                var employee = new Employee
                {
                    UserId = user.Id,
                    StoreId = storeId
                };
                _context.Employees.Add(employee);
            }

            await _context.SaveChangesAsync();

            // Log the action
            await LogAuditAsync("Assign Employee", $"Assigned Employee {userEmail} to Store {store.StoreName}");

            return Ok(new { Message = "Employee assigned successfully" });
        }

        [HttpGet("get-users")]
        public async Task<IActionResult> GetUsers([FromQuery] string role = null)
        {
            // Check if user is SuperAdmin or StoreAdmin
            if (!User.IsInRole("SuperAdmin") && !User.IsInRole("StoreAdmin"))
            {
                return Forbid();
            }

            var currentUserEmail = User.FindFirstValue(ClaimTypes.Email);

            if (User.IsInRole("SuperAdmin"))
            {
                // SuperAdmin can filter by role
                var query = _context.Users.Include(u => u.Role).AsQueryable();
                
                // Apply role filtering for SuperAdmin
                if (!string.IsNullOrEmpty(role))
                {
                    if (role.Equals("StoreAdmin", StringComparison.OrdinalIgnoreCase))
                    {
                        query = query.Where(u => u.RoleId == UserRole.StoreAdmin);
                    }
                    else if (role.Equals("Employee", StringComparison.OrdinalIgnoreCase))
                    {
                        query = query.Where(u => u.RoleId == UserRole.Employee);
                    }
                    else if (role.Equals("Customer", StringComparison.OrdinalIgnoreCase))
                    {
                        query = query.Where(u => u.RoleId == UserRole.Customer);
                    }
                    else
                    {
                        // Default to showing StoreAdmins and Employees if invalid role
                        query = query.Where(u => u.RoleId == UserRole.StoreAdmin || u.RoleId == UserRole.Employee);
                    }
                }
                else
                {
                    // Default behavior - show StoreAdmins and Employees
                    query = query.Where(u => u.RoleId == UserRole.StoreAdmin || u.RoleId == UserRole.Employee);
                }
                
                var users = await query
                    .Select(u => new
                    {
                        u.Id,
                        u.Username,
                        u.Email,
                        u.FirstName,
                        u.LastName,
                        u.PhoneNumber,
                        Role = u.Role.Name,
                        u.CreatedAt
                    })
                    .ToListAsync();

                return Ok(users);
            }
            else
            {
                // StoreAdmin only sees Employees assigned to their store
                // Role filter doesn't apply for StoreAdmin - they can only see employees
                var storeAdmin = await _context.Stores
                    .FirstOrDefaultAsync(s => s.StoreAdmin == currentUserEmail);

                if (storeAdmin == null)
                {
                    return BadRequest(new { Message = "Store Admin not assigned to any store" });
                }

                var employees = await _context.Employees
                    .Where(e => e.StoreId == storeAdmin.StoreId)
                    .Include(e => e.User)
                    .Select(e => new
                    {
                        e.User.Id,
                        e.User.Username,
                        e.User.Email,
                        e.User.FirstName,
                        e.User.LastName,
                        e.User.PhoneNumber,
                        Role = "Employee",
                        e.User.CreatedAt
                    })
                    .ToListAsync();

                return Ok(employees);
            }
        }

        private async Task LogAuditAsync(string action, string details)
        {
            var auditLog = new AuditLog
            {
                Userid = User.FindFirstValue(ClaimTypes.NameIdentifier),
                Action = action,
                Details = details,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = HttpContext.Request.Headers["User-Agent"].ToString(),
                CreatedAt = DateTime.UtcNow
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
        }
    }
}

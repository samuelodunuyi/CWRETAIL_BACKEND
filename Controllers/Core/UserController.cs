using CWSERVER.Data;
using CWSERVER.Models.Core.DTOs;
using CWSERVER.Models.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;

namespace CWSERVER.Controllers.Core
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController(UserManager<User> userManager, ApiDbContext context) : ControllerBase
    {
        private readonly UserManager<User> _userManager = userManager;
        private readonly ApiDbContext _context = context;

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var userExists = await _userManager.FindByEmailAsync(request.Email!);
            if (userExists != null)
                return BadRequest("User already exists");

           
            var role = "Customer";
            var createdBy = (string?)null;

            if (User.Identity?.IsAuthenticated == true)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                    return Unauthorized();

               
                if (currentUser.Role == "Admin" && !string.IsNullOrEmpty(request.Role))
                {
                    role = string.IsNullOrWhiteSpace(request.Role)
                        ? string.Empty
                        : char.ToUpper(request.Role[0]) + request.Role[1..].ToLower();

                }
                createdBy = currentUser.Email;
            }

            var user = new User
            {
                Email = request.Email,
                UserName = request.Email,
                Role = role,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                LastUpdatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, request.Password!);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

           
            var customer = new Customer
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                UserId = user.Id,
                CreatedBy = createdBy
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            return Ok("User created successfully");
        }



        [Authorize(Roles = "Admin")]
        [HttpPost("register-employee")]
        public async Task<IActionResult> RegisterEmployee([FromBody] RegisterEmployeeRequest request)
        {
            var userExists = await _userManager.FindByEmailAsync(request.Email!);
            if (userExists != null)
                return BadRequest("User already exists");

            
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null || currentUser.Role != "Admin")
                return Forbid();

            var user = new User
            {
                Email = request.Email,
                UserName = request.Email,
                Role = string.IsNullOrWhiteSpace(request.Role)
                    ? "Employee"
                    : char.ToUpper(request.Role[0]) + request.Role[1..].ToLower(),
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                LastUpdatedAt = DateTime.UtcNow,
                LastUpdatedBy = currentUser.Email
            };


            var result = await _userManager.CreateAsync(user, request.Password!);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

          
            var employee = new Employee
            {
                UserId = user.Id,
                StoreId = request.StoreId,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PhoneNumber = request.PhoneNumber
            };

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            return Ok("Employee created successfully");
        }




        [Authorize]
        [HttpGet("userdata")]
        public async Task<IActionResult> GetCurrentUserData()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId!);

            if (user == null)
                return NotFound("User not found");

            var response = new UserDataResponse
            {
                Id = user.Id,
                Email = user.Email,
                Role = user.Role,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                LastUpdatedAt = user.LastUpdatedAt,
                LastUpdatedBy = user.LastUpdatedBy,

            };

         
            if (user.Role is "Admin" or "Employee" or "StoreRep")
            {
                var employee = await _context.Employees
                    .Include(e => e.Store)
                    .FirstOrDefaultAsync(e => e.UserId == user.Id);

                if (employee != null)
                {
                    response.FirstName = employee.FirstName;
                    response.LastName = employee.LastName;
                    response.PhoneNumber = employee.PhoneNumber;
                    response.StoreId = employee.Store?.StoreId;
                    response.StoreName = employee.Store?.StoreName;

                    /*
                    response.EmployeeData = new EmployeeDto
                    {
                        Id = employee.Id,
                        //StoreId = employee.StoreId,
                        Store = employee.Store != null ? new StoreDto
                        {
                            StoreId = employee.Store.StoreId,
                            StoreName = employee.Store.StoreName,
                            StoreRep = employee.Store.StoreRep
                        } : null,
                        FirstName = employee.FirstName,
                        LastName = employee.LastName,
                        PhoneNumber = employee.PhoneNumber
                    };*/
                }
            }

     
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (customer != null)
            {

                response.FirstName = customer.FirstName;
                response.LastName = customer.LastName;
                response.PhoneNumber = customer.PhoneNumber;

                response.CustomerData = new CustomerDto
                {
                    Id = customer.Id,
                    FirstName = customer.FirstName,
                    LastName = customer.LastName,
                    Email = customer.Email,
                    PhoneNumber = customer.PhoneNumber,
                    CreatedBy = customer.CreatedBy,
                    CreatedAt = customer.CreatedAt
                };
            }

            return Ok(response);
        }


    }
}
using CW_RETAIL.Data;
using CW_RETAIL.Models.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CW_RETAIL.Controllers.Core
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class StoreController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public StoreController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Store
        [HttpGet]
        public async Task<IActionResult> GetStores()
        {
            var currentUserRole = User.FindFirstValue(ClaimTypes.Role);
            var currentUserEmail = User.FindFirstValue(ClaimTypes.Email);

            if (currentUserRole == "0")
            {
                // SuperAdmin sees all stores
                var stores = await _context.Stores.ToListAsync();
                return Ok(stores);
            }
            else if (currentUserRole == "StoreAdmin")
            {
                // StoreAdmin sees only their store
                var stores = await _context.Stores
                    .Where(s => s.StoreAdmin == currentUserEmail)
                    .ToListAsync();
                return Ok(stores);
            }
            else if (currentUserRole == "Employee")
            {
                // Employee sees only their assigned store
                var employee = await _context.Employees
                    .Include(e => e.Store)
                    .FirstOrDefaultAsync(e => e.User.Email == currentUserEmail);

                if (employee == null || employee.Store == null)
                {
                    return NotFound(new { Message = "Employee not assigned to any store" });
                }

                return Ok(new[] { employee.Store });
            }

            return Forbid();
        }

        // GET: api/Store/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetStore(int id)
        {
            var store = await _context.Stores.FindAsync(id);

            if (store == null)
            {
                return NotFound();
            }

            var currentUserRole = User.FindFirstValue(ClaimTypes.Role);
            var currentUserEmail = User.FindFirstValue(ClaimTypes.Email);

            // Check permissions
            if (currentUserRole == "SuperAdmin")
            {
                // SuperAdmin can access any store
                return Ok(store);
            }
            else if (currentUserRole == "StoreAdmin" && store.StoreAdmin == currentUserEmail)
            {
                // StoreAdmin can only access their store
                return Ok(store);
            }
            else if (currentUserRole == "Employee")
            {
                // Employee can only access their assigned store
                var employee = await _context.Employees
                    .FirstOrDefaultAsync(e => e.User.Email == currentUserEmail && e.StoreId == id);

                if (employee != null)
                {
                    return Ok(store);
                }
            }

            return Forbid();
        }

        // POST: api/Store
        [HttpPost]
        public async Task<IActionResult> CreateStore([FromBody] Store store)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUserRole = User.FindFirstValue(ClaimTypes.Role);
            var currentUserEmail = User.FindFirstValue(ClaimTypes.Email);
            
            store.CreatedAt = DateTime.UtcNow;
            _context.Stores.Add(store);
            await _context.SaveChangesAsync();

            // Log the action
            await LogAuditAsync("Create Store", $"Created store: {store.StoreName}");

            return CreatedAtAction(nameof(GetStore), new { id = store.StoreId }, store);
        }

        // PUT: api/Store/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStore(int id, Store store)
        {
            if (id != store.StoreId)
            {
                return BadRequest();
            }

            var existingStore = await _context.Stores.FindAsync(id);
            if (existingStore == null)
            {
                return NotFound();
            }

            var currentUserRole = User.FindFirstValue(ClaimTypes.Role);
            var currentUserEmail = User.FindFirstValue(ClaimTypes.Email);

            // Check permissions
            if (currentUserRole == "SuperAdmin")
            {
                // SuperAdmin can update any store
            }
            else if (currentUserRole == "StoreAdmin" && existingStore.StoreAdmin == currentUserEmail)
            {
                // StoreAdmin can only update their store
                // But they cannot change the StoreAdmin field
                store.StoreAdmin = existingStore.StoreAdmin;
                store.UserId = existingStore.UserId;
            }
            else
            {
                return Forbid();
            }

            // Update fields
            existingStore.StoreName = store.StoreName;
            existingStore.StorePhoneNumber = store.StorePhoneNumber;
            existingStore.StoreEmailAddress = store.StoreEmailAddress;
            existingStore.StoreAddress = store.StoreAddress;
            existingStore.StoreType = store.StoreType;
            
            // Only SuperAdmin can update these fields
            if (currentUserRole == "SuperAdmin")
            {
                existingStore.StoreAdmin = store.StoreAdmin;
                existingStore.UserId = store.UserId;
            }
            
            existingStore.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
                
                // Log the action
                await LogAuditAsync("Update Store", $"Updated store: {store.StoreName}");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StoreExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // PATCH: api/Store/5
        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchStore(int id, [FromBody] Store patchStore)
        {
            var store = await _context.Stores.FindAsync(id);
            if (store == null)
            {
                return NotFound();
            }

            var currentUserRole = User.FindFirstValue(ClaimTypes.Role);
            var currentUserEmail = User.FindFirstValue(ClaimTypes.Email);

            // Check permissions
            if (currentUserRole == "SuperAdmin")
            {
                // SuperAdmin can update any store
            }
            else if (currentUserRole == "StoreAdmin" && store.StoreAdmin == currentUserEmail)
            {
                // StoreAdmin can only update their store
                // But they cannot change the StoreAdmin field
                patchStore.StoreAdmin = store.StoreAdmin;
                patchStore.UserId = store.UserId;
            }
            else
            {
                return Forbid();
            }

            // Update non-null fields
            if (patchStore.StoreName != null)
                store.StoreName = patchStore.StoreName;
            if (patchStore.StorePhoneNumber != null)
                store.StorePhoneNumber = patchStore.StorePhoneNumber;
            if (patchStore.StoreEmailAddress != null)
                store.StoreEmailAddress = patchStore.StoreEmailAddress;
            if (patchStore.StoreAddress != null)
                store.StoreAddress = patchStore.StoreAddress;
            if (patchStore.StoreType != null)
                store.StoreType = patchStore.StoreType;
            
            // Only SuperAdmin can update these fields
            if (currentUserRole == "SuperAdmin")
            {
                if (patchStore.StoreAdmin != null)
                    store.StoreAdmin = patchStore.StoreAdmin;
                if (patchStore.UserId != null)
                    store.UserId = patchStore.UserId;
            }
            
            store.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            
            // Log the action
            await LogAuditAsync("Patch Store", $"Updated store: {store.StoreName}");

            return NoContent();
        }

        // DELETE: api/Store/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> DeleteStore(int id)
        {
            var store = await _context.Stores.FindAsync(id);
            if (store == null)
            {
                return NotFound();
            }

            // Check if store has employees
            var hasEmployees = await _context.Employees.AnyAsync(e => e.StoreId == id);
            if (hasEmployees)
            {
                return BadRequest(new { Message = "Cannot delete store with assigned employees" });
            }

            _context.Stores.Remove(store);
            await _context.SaveChangesAsync();
            
            // Log the action
            await LogAuditAsync("Delete Store", $"Deleted store: {store.StoreName}");

            return NoContent();
        }

        private bool StoreExists(int id)
        {
            return _context.Stores.Any(e => e.StoreId == id);
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

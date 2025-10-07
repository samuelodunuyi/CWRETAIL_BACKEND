using CW_RETAIL.Data;
using CW_RETAIL.Models.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace CW_RETAIL.Controllers.Core
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "SuperAdmin,StoreAdmin")]
    public class AuditLogController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AuditLogController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/AuditLog
        [HttpGet]
        public async Task<IActionResult> GetAuditLogs([FromQuery] string userId = null, [FromQuery] string action = null)
        {
            var query = _context.AuditLogs.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(userId))
            {
                query = query.Where(a => a.Userid == userId);
            }

            if (!string.IsNullOrEmpty(action))
            {
                query = query.Where(a => a.Action.Contains(action));
            }

            // Order by creation date (newest first)
            query = query.OrderByDescending(a => a.CreatedAt);

            var auditLogs = await query.ToListAsync();
            return Ok(auditLogs);
        }

        // GET: api/AuditLog/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAuditLog(int id)
        {
            var auditLog = await _context.AuditLogs.FindAsync(id);

            if (auditLog == null)
            {
                return NotFound();
            }

            return Ok(auditLog);
        }
    }
}

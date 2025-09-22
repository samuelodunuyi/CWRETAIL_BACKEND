using CWSERVER.Data;
using CWSERVER.Interfaces.Industry.Restaurant;
using CWSERVER.Respository.Industry.Restaurant;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CWSERVER.Controllers.Industries.Restaurants
{
    [Route("api/[controller]/restaurant/[controller]")]
    [ApiController]
    public class AuditLogsController(IAuditLogs auditLogsRepo) : ControllerBase
    {
        private readonly IAuditLogs _auditLogsRepo = auditLogsRepo;

        [HttpGet]
        public async Task<IActionResult> GetAllLogs()
        {
            try
            {
                var result = await _auditLogsRepo.GetAllAuditLogsAsyncs();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }
    }
}

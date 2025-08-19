using CWSERVER.Data;
using CWSERVER.Interfaces.Industry.Restaurant;
using CWSERVER.Models.Industries.Restaurant.Entities;
using Microsoft.EntityFrameworkCore;

namespace CWSERVER.Respository.Industry.Restaurant
{
    public class AuditLogsRepository(ApiDbContext dbContext) : IAuditLogs
    {
        private readonly ApiDbContext _dbContext = dbContext;
        public async Task<List<AuditLogs>> GetAllAuditLogsAsyncs()
        {
            return await _dbContext.AuditLogss.ToListAsync();
        }
    }
}

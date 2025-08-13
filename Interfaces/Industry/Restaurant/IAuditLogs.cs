using CWSERVER.Models.Industries.Restaurant.Entities;

namespace CWSERVER.Interfaces.Industry.Restaurant
{
    public interface IAuditLogs
    {
        Task<List<AuditLogs>> GetAllAuditLogsAsyncs();
    }
}

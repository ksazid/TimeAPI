using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface IEntityHistoryLogRepository : IRepositoryAsync<EntityHistoryLog, string>
    {
        Task<dynamic> EntityHistoryLogByEntityID(string EntityID);
    }
}

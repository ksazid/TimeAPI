using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;
using System.Threading.Tasks;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Model;

namespace TimeAPI.Domain.Repositories
{
    public interface ICostPerHourRepository : IRepositoryAsync<CostPerHour, string>
    {
        Task<IEnumerable<CostPerHour>> FetchCostPerHourOrgID(string OrgID);
    }
}

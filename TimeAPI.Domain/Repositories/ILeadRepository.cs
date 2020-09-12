using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface ILeadRepository : IRepositoryAsync<Lead, string>
    {
        Task<dynamic> LeadByOrgID(string OrgID);
        Task<dynamic> FindByLeadID(string LeadID);
        Task UpdateLeadStatusByLeadID(Lead entity);

    }
}

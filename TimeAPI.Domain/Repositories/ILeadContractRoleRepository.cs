using System;
using System.Collections.Generic;
using System.Text;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface ILeadContractRoleRepository : IRepository<LeadContractRole, string>
    {
        IEnumerable<LeadContractRole> GetLeadContractRoleByOrgID(string OrgID);
    }
}

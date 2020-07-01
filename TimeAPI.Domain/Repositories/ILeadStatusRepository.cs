using System;
using System.Collections.Generic;
using System.Text;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface ILeadStatusRepository : IRepository<LeadStatus, string>
    {
        public IEnumerable<LeadStatus> LeadStatusByOrgID(string OrgID);
    }
}

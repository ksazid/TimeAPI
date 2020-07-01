using System;
using System.Collections.Generic;
using System.Text;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface ILeadSourceRepository : IRepository<LeadSource, string>
    {
        public IEnumerable<LeadSource> LeadSourceByOrgID(string OrgID);
    }
}

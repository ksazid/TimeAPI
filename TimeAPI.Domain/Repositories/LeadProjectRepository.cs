using System;
using System.Collections.Generic;
using System.Text;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface ILeadProjectRepository : IRepository<LeadProject, string>
    {
        public IEnumerable<LeadProject> LeadProjectByOrgID(string OrgID);
        public LeadProject LeadProjectByLeadID(string LeadID);
        
    }
}

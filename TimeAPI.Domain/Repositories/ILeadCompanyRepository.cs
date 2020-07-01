using System;
using System.Collections.Generic;
using System.Text;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface ILeadCompanyRepository : IRepository<LeadCompany, string>
    {
        public IEnumerable<LeadCompany> LeadCompanyByOrgID(string OrgID);
    }
}

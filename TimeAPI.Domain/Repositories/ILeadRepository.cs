using System;
using System.Collections.Generic;
using System.Text;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface ILeadRepository : IRepository<Lead, string>
    {
        dynamic LeadByOrgID(string OrgID);
        dynamic FindByLeadID(string LeadID);
        void UpdateLeadStatusByLeadID(Lead entity);

        
    }
}

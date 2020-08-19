using System;
using System.Collections.Generic;
using System.Text;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface ILeadRepository : IRepository<Lead, string>
    {
        public dynamic LeadByOrgID(string OrgID);
        void UpdateLeadStatusByLeadID(Lead entity);
    }
}

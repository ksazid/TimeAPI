using System;
using System.Collections.Generic;
using System.Text;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface ILeadDealRepository : IRepository<LeadDeal, string>
    {
        public IEnumerable<LeadDeal> LeadDealByOrgID(string OrgID);
        public LeadDeal LeadDealByLeadID(string LeadID);
        void UpdateEstDealValueByLeadID(LeadDeal entity);
    }
}

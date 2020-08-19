using System;
using System.Collections.Generic;
using System.Text;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface ILeadDealTypeRepository : IRepository<LeadDealType, string>
    {
        IEnumerable<LeadDealType> GetLeadDealTypeByOrgID(string OrgID);
    }
}

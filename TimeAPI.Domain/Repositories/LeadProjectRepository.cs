using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface ILeadDealRepository : IRepositoryAsync<LeadDeal, string>
    {
        Task<IEnumerable<LeadDeal>> LeadDealByOrgID(string OrgID);
        Task<LeadDeal> LeadDealByLeadID(string LeadID);
        Task UpdateEstDealValueByLeadID(LeadDeal entity);
        Task<string> GetLastAddedLeadPrefixByOrgID(string OrgID);
        Task RemoveByLeadID(string LeadID);


    }
}

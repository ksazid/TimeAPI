using System;
using System.Collections.Generic;
using System.Text;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface ILeadStageRepository : IRepository<LeadStage, string>
    {
        IEnumerable<LeadStage> GetLeadStageByOrgID(string OrgID);
    }
}

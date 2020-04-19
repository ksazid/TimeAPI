using System;
using System.Collections.Generic;
using System.Text;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface IMilestoneTemplateRepository : IRepository<MilestoneTemplate, string>
    {
        //void RemoveByOrgID(string OrgID);
        IEnumerable<MilestoneTemplate> FindByOrgID(string OrgID);
    }
}

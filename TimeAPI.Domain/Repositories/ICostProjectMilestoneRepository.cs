using System;
using System.Collections.Generic;
using System.Text;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface ICostProjectMilestoneRepository : IRepository<CostProjectMilestone, string>
    {
        
        IEnumerable<CostProjectMilestone> GetCostProjectMilestoneByProjectID(string ProjectID);
        //void UpdateCostProjectMilestoneStatusByActivityID(CostProjectMilestone entity);
        //dynamic GetCostProjectMilestoneRatioByProjectID(string ProjectID);
        IEnumerable<CostProjectMilestone> GetAllStaticMilestoneByOrgID(string OrgID);
    }
}

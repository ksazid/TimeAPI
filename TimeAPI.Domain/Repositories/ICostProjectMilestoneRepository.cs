using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface ICostProjectMilestoneRepository : IRepositoryAsync<CostProjectMilestone, string>
    {
        
        Task< IEnumerable<CostProjectMilestone>> GetCostProjectMilestoneByProjectID(string ProjectID);
        //void UpdateCostProjectMilestoneStatusByActivityID(CostProjectMilestone entity);
        //dynamic GetCostProjectMilestoneRatioByProjectID(string ProjectID);
        Task<IEnumerable<CostProjectMilestone>> GetAllStaticMilestoneByOrgID(string OrgID);
        Task RemoveByProjectID(string ProjectID);
    }
}

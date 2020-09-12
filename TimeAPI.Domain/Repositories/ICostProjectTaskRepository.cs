using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;
using System.Threading.Tasks;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Model;

namespace TimeAPI.Domain.Repositories
{
    public interface ICostProjectTaskRepository : IRepositoryAsync<CostProjectTask, string>
    {
        Task<IEnumerable<CostProjectTask>> GetAllStaticMilestoneTasksByMilestoneID(string MilestoneID, string OrgID);
        Task<IEnumerable<CostProjectTask>> GetAllMilestoneTasksByMilestoneID(string MilestoneID, string OrgID);

        //IEnumerable<CostProjectTask> GetAllStaticMilestoneByOrgID(string MilestoneID);
        Task UpdateStaticCostProjectTask(CostProjectTask entity);
        Task RemoveByProjectID(string ProjectID);
        Task UpdateIsSelectedByTaskID(CostProjectTask entity);
        Task UpdateCostProjectTaskQtyTaskID(CostProjectTask entity);
        Task UpdateCostProjectNotesQtyTaskID(CostProjectTask entity);
        Task UpdateCostProjectBudgetedHoursTaskID(CostProjectTask entity);
        Task UpdateCostProjectDiscountAndTotalCostTaskID(CostProjectTask entity);
    }
}

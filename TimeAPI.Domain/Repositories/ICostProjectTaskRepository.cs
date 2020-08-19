using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Model;

namespace TimeAPI.Domain.Repositories
{
    public interface ICostProjectTaskRepository : IRepository<CostProjectTask, string>
    {
        //dynamic FindByTaskDetailsByEmpID(string empid);

        //void UpdateTaskStatus(CostProjectTask entity);

        //RootEmployeeTask GetAllTaskByEmpID(string empid, string date);

        //RootEmployeeTask GetAllTaskByOrgAndEmpID(string key, string EmpID);
        IEnumerable<CostProjectTask> GetAllStaticMilestoneTasksByMilestoneID(string MilestoneID, string OrgID);
        IEnumerable<CostProjectTask> GetAllMilestoneTasksByMilestoneID(string MilestoneID, string OrgID);

        //IEnumerable<CostProjectTask> GetAllStaticMilestoneByOrgID(string MilestoneID);

        void UpdateStaticCostProjectTask(CostProjectTask entity);

        void RemoveByProjectID(string ProjectID);
        void UpdateIsSelectedByTaskID(CostProjectTask entity);
        void UpdateCostProjectTaskQtyTaskID(CostProjectTask entity);
        void UpdateCostProjectNotesQtyTaskID(CostProjectTask entity);
        void UpdateCostProjectBudgetedHoursTaskID(CostProjectTask entity);
        void UpdateCostProjectDiscountAndTotalCostTaskID(CostProjectTask entity);
    }
}

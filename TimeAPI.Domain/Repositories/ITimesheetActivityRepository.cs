using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Model;

namespace TimeAPI.Domain.Repositories
{
    public interface ITimesheetActivityRepository : IRepositoryAsync<TimesheetActivity, string>
    {
        //IEnumerable<Team> FindTeamsByOrgID(string OrgID);
        Task RemoveByGroupID(string GroupID);

        Task<dynamic> GetTop10TimesheetActivityOnTaskID(string TaskID);

        Task<IEnumerable<ViewLogDataModel>> GetTimesheetActivityByGroupAndProjectID(string GroupID, string ProjectID, string Date);

        Task<IEnumerable<ViewLogDataModel>> GetTimesheetActivityByEmpID(string EmpID, string StartDate, string EndDate);

    }
}

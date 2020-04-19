using System;
using System.Collections.Generic;
using System.Text;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Model;

namespace TimeAPI.Domain.Repositories
{
    public interface ITimesheetActivityRepository : IRepository<TimesheetActivity, string>
    {
        //IEnumerable<Team> FindTeamsByOrgID(string OrgID);
        void RemoveByGroupID(string GroupID);

        dynamic GetTop10TimesheetActivityOnTaskID(string TaskID);

        IEnumerable<ViewLogDataModel> GetTimesheetActivityByGroupAndProjectID(string GroupID, string ProjectID, string Date);

        IEnumerable<ViewLogDataModel> GetTimesheetActivityByEmpID(string EmpID, string StartDate, string EndDate);

    }
}

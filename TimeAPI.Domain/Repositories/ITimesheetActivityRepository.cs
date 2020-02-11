using System;
using System.Collections.Generic;
using System.Text;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface ITimesheetActivityRepository : IRepository<TimesheetActivity, string>
    {
        //IEnumerable<Team> FindTeamsByOrgID(string OrgID);
        void RemoveByGroupID(string GroupID);

        dynamic GetTop10TimesheetActivityOnTaskID(string TaskID);

        dynamic GetTimesheetActivityByGroupAndProjectID(string GroupID, string ProjectID);

        //IEnumerable<dynamic> FetchAllTeamsByOrgID(string OrgID);
        //IEnumerable<dynamic> FetchAllTeamMembersByTeamID(string key);
        //dynamic GetAllTeamMembersByTeamID(string key);

    }
}

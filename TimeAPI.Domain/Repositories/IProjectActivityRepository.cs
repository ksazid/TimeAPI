using System;
using System.Collections.Generic;
using System.Text;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface IProjectActivityRepository : IRepository<ProjectActivity, string>
    {
        //IEnumerable<Team> FindTeamsByOrgID(string OrgID);
        //dynamic FindByTeamID(string TeamID);
        //IEnumerable<dynamic> FetchAllTeamsByOrgID(string OrgID);
        //IEnumerable<dynamic> FetchAllTeamMembersByTeamID(string key);
        IEnumerable<ProjectActivity> GetProjectActivityByProjectID(string ProjectID);
        void UpdateProjectActivityStatusByActivityID(ProjectActivity entity);
        //IEnumerable<ProjectActivity> GetProjectActivityByProjectID(string ProjectID);

    }
}

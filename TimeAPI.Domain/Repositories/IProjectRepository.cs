using System;
using System.Collections.Generic;
using System.Text;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface IProjectRepository : IRepository<Project, string>
    {
        //IEnumerable<Team> FindTeamsByOrgID(string OrgID);
        //dynamic FindByTeamID(string TeamID);
        IEnumerable<dynamic> FetchAllProjectByOrgID(string OrgID);
        void UpdateProjectStatusByID(Project entity);
        Project FindAutoProjectPrefixByOrgID(string key, string key1);
        Project FindCustomProjectPrefixByOrgIDAndPrefix(string key, string key1);
        string ProjectTaskCount(string key);
        IEnumerable<dynamic> FindAllProjectActivityByProjectID(string ProjectID);
        //string ProjectActivityCount(string key);

    }
}

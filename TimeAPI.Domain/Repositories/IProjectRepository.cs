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
        //IEnumerable<dynamic> FetchAllTeamMembersByTeamID(string key);
        //dynamic GetAllTeamMembersByTeamID(string key);
    }
}

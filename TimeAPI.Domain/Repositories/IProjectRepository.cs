using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface IProjectRepository : IRepositoryAsync<Project, string>
    {
        //IEnumerable<Team> FindTeamsByOrgID(string OrgID);
        //dynamic FindByTeamID(string TeamID);
        Task< IEnumerable<dynamic>> FetchAllProjectByOrgID(string OrgID);
        Task UpdateProjectStatusByID(Project entity);
        Task<Project> FindAutoProjectPrefixByOrgID(string key, string key1);
        Task<Project> FindAutoCostProjectPrefixByOrgID(string key, string key1);
        Task<Project> FindCustomProjectPrefixByOrgIDAndPrefix(string key, string key1);
        Task<string> ProjectTaskCount(string key);
        Task<IEnumerable<dynamic>> FindAllProjectActivityByProjectID(string ProjectID);
        Task<string> GetLastAddedProjectPrefixByOrgID(string key);
       
        

    }
}

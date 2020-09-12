using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface ITeamRepository : IRepositoryAsync<Team, string>
    {
       Task<IEnumerable<dynamic>> FindTeamsByOrgID(string OrgID);
        Task<dynamic> FindByTeamID(string TeamID);
        Task<IEnumerable<dynamic>> FetchAllTeamsByOrgID(string OrgID);
        Task<IEnumerable<dynamic>> FetchAllTeamMembersByTeamID(string key);
        Task<dynamic> GetAllTeamMembersByTeamID(string key);
    }
}

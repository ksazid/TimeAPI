using System;
using System.Collections.Generic;
using System.Text;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface ITeamRepository : IRepository<Team, string>
    {
        IEnumerable<dynamic> FindTeamsByOrgID(string OrgID);
        dynamic FindByTeamID(string TeamID);
        IEnumerable<dynamic> FetchAllTeamsByOrgID(string OrgID);
        IEnumerable<dynamic> FetchAllTeamMembersByTeamID(string key);
        dynamic GetAllTeamMembersByTeamID(string key);

    }
}

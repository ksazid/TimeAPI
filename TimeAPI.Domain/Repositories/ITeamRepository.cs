using System;
using System.Collections.Generic;
using System.Text;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface ITeamRepository : IRepository<Team, string>
    {
        IEnumerable<Team> FindTeamsByOrgID(string OrgID);
        Team FindByTeamID(string TeamID);
        IEnumerable<dynamic> FetchByAllTeamMembersTeamID(string TeamID);

        IEnumerable<dynamic> FetchAllTeamsByOrgID(string OrgID);

    }
}

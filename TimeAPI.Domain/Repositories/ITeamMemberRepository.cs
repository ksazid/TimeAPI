using System;
using System.Collections.Generic;
using System.Text;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface ITeamMemberRepository : IRepository<TeamMembers, string>
    {
        void RemoveByTeamID(string key);
    }
}

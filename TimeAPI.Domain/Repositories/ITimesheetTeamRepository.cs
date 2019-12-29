using System;
using System.Collections.Generic;
using System.Text;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface ITimesheetTeamRepository : IRepository<TimesheetTeam, string>
    {
        void RemoveByGroupID(string GroupID);
    }
}

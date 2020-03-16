using System;
using System.Collections.Generic;
using System.Text;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface IWeekendHoursRepository : IRepository<WeekendHours, string>
    {
        void RemoveByOrgID(string OrgID);
    }
}

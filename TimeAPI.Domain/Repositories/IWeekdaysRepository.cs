using System;
using System.Collections.Generic;
using System.Text;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface IWeekdaysRepository : IRepository<Weekdays, string>
    {
        void RemoveByOrgID(string OrgID);
        IEnumerable<Weekdays> FindByOrgID(string OrgID);
    }
}

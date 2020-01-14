using System;
using System.Collections.Generic;
using System.Text;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface IPriorityRepository : IRepository<Priority, string>
    {
        public IEnumerable<Priority> GetPriorityByOrgID(string OrgID);
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface ILocalActivityRepository : IRepository<LocalActivity, string>
    {
        IEnumerable<LocalActivity> LocalActivityByOrgID(string OrgID);
    }
}

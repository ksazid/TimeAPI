using System;
using System.Collections.Generic;
using System.Text;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface IExclusionRepository : IRepository<Exclusion, string>
    {
        public IEnumerable<Exclusion> ExclusionByOrgID(string OrgID);
    }
}

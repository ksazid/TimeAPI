using System;
using System.Collections.Generic;
using System.Text;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface IProjectTypeRepository : IRepository<ProjectType, string>
    {
        void RemoveByOrgID(string OrgID);
        IEnumerable<ProjectType> FindByOrgID(string OrgID);
    }
}

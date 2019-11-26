using System;
using System.Collections.Generic;
using System.Text;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface IDepartmentRepository : IRepository<Department, string>
    {
        Department FindByDepartmentName(string dep_name);
        Department FindByDepartmentAlias(string alias);
        IEnumerable<Department> FindDepartmentByOrgID(string OrgID);
    }
}

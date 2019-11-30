using System;
using System.Collections.Generic;
using System.Text;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface IPriorityRepository : IRepository<Priority, string>
    {
        //Department FindByDepartmentName(string dep_name);
        //Department FindByDepartmentAlias(string alias);
        //IEnumerable<Department> FindDepartmentByOrgID(string OrgID);
        //IEnumerable<DepartmentResultSet> FindAllDepLeadByOrgID(string OrgID);
        //DepartmentResultSet FindDepLeadByDepID(string OrgID);
    }
}

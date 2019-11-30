using System;
using System.Collections.Generic;
using System.Text;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Model;

namespace TimeAPI.Domain.Repositories
{
    public interface ITaskRepository : IRepository<Task, string>
    {
        //Department FindByDepartmentName(string dep_name);
        //Department FindByDepartmentAlias(string alias);
        //IEnumerable<Department> FindDepartmentByOrgID(string OrgID);
        //IEnumerable<DepartmentResultSet> FindAllDepLeadByOrgID(string OrgID);
        //DepartmentResultSet FindDepLeadByDepID(string OrgID);
    }
}

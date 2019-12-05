using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Model;

namespace TimeAPI.Domain.Repositories
{
    public interface ITaskRepository : IRepository<Tasks, string>
    {
        dynamic FindByTaskDetailsByEmpID(string empid);
        //Department FindByDepartmentAlias(string alias);
        //IEnumerable<Department> FindDepartmentByOrgID(string OrgID);
        //IEnumerable<DepartmentResultSet> FindAllDepLeadByOrgID(string OrgID);
        //DepartmentResultSet FindDepLeadByDepID(string OrgID);
    }
}

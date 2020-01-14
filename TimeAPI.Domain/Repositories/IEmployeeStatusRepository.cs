using System;
using System.Collections.Generic;
using System.Text;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface IEmployeeStatusRepository : IRepository<EmployeeStatus, string>
    {
        //Department FindByDepartmentName(string dep_name);
        //Department FindByDepartmentAlias(string alias);
        //IEnumerable<Department> FindDepartmentByOrgID(string OrgID);
        IEnumerable<EmployeeStatus> GetEmployeeStatusByOrgID(string OrgID);
        //DepartmentResultSet FindDepLeadByDepID(string OrgID);
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Model;

namespace TimeAPI.Domain.Repositories
{
    public interface IDepartmentRepository : IRepository<Department, string>
    {
        Department FindByDepartmentName(string dep_name);
        Department FindByDepartmentAlias(string alias);
        IEnumerable<Department> FindDepartmentByOrgID(string OrgID);
        dynamic FindAllDepLeadByOrgID(string OrgID);
        dynamic FindAllDepMembersByDepID(string DepID);
        dynamic FindDepLeadByDepID(string DepID);
        dynamic FetchGridDataByDepOrgID(string OrgID);

    }
}

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
        IEnumerable<dynamic> FindAllDepLeadByOrgID(string OrgID);
        dynamic FindDepLeadByDepID(string OrgID);
        dynamic FetchGridDataByDepOrgID(string OrgID);

    }
}

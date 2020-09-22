using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Model;

namespace TimeAPI.Domain.Repositories
{
    public interface IDepartmentRepository : IRepositoryAsync<Department, string>
    {
        Task<Department> FindByDepartmentName(string dep_name);
        Task<Department> FindByDepartmentAlias(string alias);
        Task<IEnumerable<Department>> FindDepartmentByOrgID(string OrgID);
        Task<dynamic> FindAllDepLeadByOrgID(string OrgID);
        Task<dynamic> FindAllDepMembersByDepID(string DepID);
        Task<dynamic> FindDepLeadByDepID(string DepID);
        Task<dynamic> FetchGridDataByDepOrgID(string OrgID);

    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface IEmployeeRepository : IRepositoryAsync<Employee, string>
    {
        Task<Employee> FindByEmpName(string full_name);
        Task<Employee> FindByEmpCode(string emp_code);
        Task<Employee> FindByEmpUserID(string UserID);
        Task SetEmpPasswordResetByUserID(string UserID);
        Task<IEnumerable<Employee>> FindByOrgIDCode(string org_id);
        Task<IEnumerable<Employee>> FindByRoleName(string role);
        Task<dynamic> FetchGridDataEmployeeByOrgID(string org_id);
        Task<dynamic> FindEmployeeListByDesignationID(string DesignationID);
        Task<dynamic> FindEmployeeListByDepartmentID(string DesignationID);
        Task<dynamic> FindEmpDepartDesignByEmpID(string EmpID);
        Task<dynamic> FindEmpDepartDesignByTeamID(string EmpID);
        Task<dynamic> GetAllOutsourcedEmpByOrgID(string OrgID);
        Task<dynamic> GetAllFreelancerEmpByOrgID(string OrgID);
        Task SetEmployeeInactiveByEmpID(string OrgID);
        Task RemovePermanent(string EmpID);
        Task SetDelegateeAsAdminByEmpID(string EmpID);
        Task SetDelegateeAsSuperAdminByEmpID(string EmpID);
        Task RemoveAdminRightByEmpID(string key);
        Task RemoveSuperAdminRightByEmpID(string key);
        Task<int> RemoveEmployeeIfZeroActivity(string key);
        Task<dynamic> GetOrganizationScreenshotDetails(string userid);
    }
}

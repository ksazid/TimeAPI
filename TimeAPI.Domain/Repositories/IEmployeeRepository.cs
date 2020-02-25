using System;
using System.Collections.Generic;
using System.Text;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface IEmployeeRepository : IRepository<Employee, string>
    {
        Employee FindByEmpName(string full_name);
        Employee FindByEmpCode(string emp_code);
        Employee FindByEmpUserID(string UserID);
        void SetEmpPasswordResetByUserID(string UserID);
        IEnumerable<Employee> FindByOrgIDCode(string org_id);
        IEnumerable<Employee> FindByRoleName(string role);
        dynamic FetchGridDataEmployeeByOrgID(string org_id);
        dynamic FindEmployeeListByDesignationID(string DesignationID);
        dynamic FindEmployeeListByDepartmentID(string DesignationID);
        dynamic FindEmpDepartDesignByEmpID(string EmpID);
        dynamic FindEmpDepartDesignByTeamID(string EmpID);
        dynamic GetAllOutsourcedEmpByOrgID(string OrgID);
        dynamic GetAllFreelancerEmpByOrgID(string OrgID);
        void SetEmployeeInactiveByEmpID(string OrgID);
        void RemovePermanent(string EmpID);
        void SetCustomerAsAdminByEmpID(string EmpID);
        void RemoveAdminRightByEmpID(string key);
    }
}

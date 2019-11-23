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
        IEnumerable<Employee> FindByOrgIDCode(string org_id);
        IEnumerable<Employee> FindByRoleName(string role);
        
            
    }
}

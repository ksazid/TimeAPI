using System;
using System.Collections.Generic;
using System.Text;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface IEmployeeAppUsageRepository : IRepository<EmployeeAppUsage, string>
    {
        public EmployeeAppUsage FindEmployeeAppUsageEmpID(string key);
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface IEmployeeAppUsageRepository : IRepositoryAsync<EmployeeAppUsage, string>
    {
        Task<EmployeeAppUsage> FindEmployeeAppUsageEmpID(string key);
    }
}

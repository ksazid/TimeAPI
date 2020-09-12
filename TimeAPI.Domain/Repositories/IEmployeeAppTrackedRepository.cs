using System;
using System.Collections.Generic;
using System.Text;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface IEmployeeAppTrackedRepository : IRepository<EmployeeAppTracked, string>
    {
         EmployeeAppTracked FindEmployeeAppTrackedEmpID(string key);
    }
}

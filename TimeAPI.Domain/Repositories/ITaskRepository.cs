using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;
using System.Threading.Tasks;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Model;

namespace TimeAPI.Domain.Repositories
{
    public interface ITaskRepository : IRepositoryAsync<Tasks, string>
    {
        Task<dynamic> FindByTaskDetailsByEmpID(string empid);

        Task UpdateTaskStatus(Tasks entity);

        Task<RootEmployeeTask> GetAllTaskByEmpID(string empid, string date);

        Task<RootEmployeeTask> GetAllTaskByOrgAndEmpID(string key, string EmpID);
    }
}

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;
using System.Threading.Tasks;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Model;

namespace TimeAPI.Domain.Repositories
{
    public interface ISubTaskRepository : IRepositoryAsync<SubTasks, string>
    {
        Task<IEnumerable<SubTasks>> FindSubTaskByTaskID(string TaskID);
        Task UpdateSubTaskStatus(SubTasks entity);
        Task UpdateSubTaskLeadBySubTaskID(SubTasks entity);
                         
        //Task<RootEmployeeTask> GetAllSubTaskByEmpID(string empid, string date);

        //Task<RootEmployeeTask> GetAllSubTaskByOrgAndEmpID(string key, string EmpID);
    }
}

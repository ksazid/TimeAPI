using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Model;

namespace TimeAPI.Domain.Repositories
{
    public interface ITaskRepository : IRepository<Tasks, string>
    {
        dynamic FindByTaskDetailsByEmpID(string empid);

        void UpdateTaskStatus(Tasks entity);

        RootEmployeeTask GetAllTaskByEmpID(string empid, string date);

        RootEmployeeTask GetAllTaskByOrgAndEmpID(string key, string EmpID);
    }
}

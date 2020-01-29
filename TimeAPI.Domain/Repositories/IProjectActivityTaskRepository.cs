using System;
using System.Collections.Generic;
using System.Text;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface IProjectActivityTaskRepository : IRepository<ProjectActivityTask, string>
    {
        void RemoveByProjectActivityID(string ProjectActivityID);
        void RemoveByProjectID(string ProjectID);
        IEnumerable<dynamic> GetAllTaskByActivityID(string ActivityID);


    }
}

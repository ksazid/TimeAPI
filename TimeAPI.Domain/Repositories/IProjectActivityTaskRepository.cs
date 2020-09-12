using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface IProjectActivityTaskRepository : IRepositoryAsync<ProjectActivityTask, string>
    {
        Task RemoveByProjectActivityID(string ProjectActivityID);
        Task RemoveByProjectID(string ProjectID);
        Task< IEnumerable<dynamic>> GetAllTaskByActivityID(string ActivityID);
        Task< IEnumerable<dynamic>> GetAllTaskByProjectID(string ProjectID);
        Task<IEnumerable<ProjectActivityTaskEntityViewModel>> GetAllTaskForAssignByProjectID(string ProjectID);
        Task<dynamic> GetProjectActivityTaskRatioByProjectID(string ProjectID);
        Task<IEnumerable<ProjectSubTaskEntityViewModel>> GetAllSubTaskByTaskID(string key);
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface IProjectActivityRepository : IRepositoryAsync<ProjectActivity, string>
    {
        Task< IEnumerable<ProjectActivity>> GetProjectActivityByProjectID(string ProjectID);
        Task UpdateProjectActivityStatusByActivityID(ProjectActivity entity);
        Task<dynamic> GetProjectActivityRatioByProjectID(string ProjectID);
        Task<dynamic> FindByProjectActivityID(string ProjectID);
    }
}

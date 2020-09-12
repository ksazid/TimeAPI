using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;
using System.Threading.Tasks;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Model;

namespace TimeAPI.Domain.Repositories
{
    public interface IProjectUnitRepository : IRepositoryAsync<ProjectUnit, string>
    {
        Task<IEnumerable<ProjectUnit>> FindByProjectID(string ProjectID);
        Task RemoveByProjectID(string ProjectID);
    }
}

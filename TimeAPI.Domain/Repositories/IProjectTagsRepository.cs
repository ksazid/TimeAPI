using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;
using System.Threading.Tasks;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Model;

namespace TimeAPI.Domain.Repositories
{
    public interface IProjectTagsRepository : IRepositoryAsync<ProjectTags, string>
    {
        Task<IEnumerable<ProjectTags>> GetProjectTagsByUnitID(string UnitID);
        Task<IEnumerable<ProjectTags>> GetProjectTagsByProjectID(string UnitID);
        Task RemoveByUnitID(string UnitID);
    }
}

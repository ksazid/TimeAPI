using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Model;

namespace TimeAPI.Domain.Repositories
{
    public interface IProjectTagsRepository : IRepository<ProjectTags, string>
    {
        IEnumerable<ProjectTags> GetProjectTagsByUnitID(string UnitID);
    }
}

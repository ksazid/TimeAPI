using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Model;

namespace TimeAPI.Domain.Repositories
{
    public interface IProjectUnitRepository : IRepository<ProjectUnit, string>
    {
        IEnumerable<ProjectUnit> FindByProjectID(string ProjectID);
        void RemoveByProjectID(string ProjectID);
    }
}

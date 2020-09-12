using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;
using System.Threading.Tasks;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Model;

namespace TimeAPI.Domain.Repositories
{
    public interface IProjectDesignTypeRepository : IRepositoryAsync<ProjectDesignType, string>
    {
       Task< IEnumerable<ProjectDesignType>> GetProjectDesignTypeByUnitID(string UnitID);
        Task<IEnumerable<ProjectDesignType>> GetProjectDesignTypeByUnitIDAndProjectTypeID(string UnitID, string ProjectTypeID);
        Task RemoveByUnitID(string UnitID);
        Task<IEnumerable<ProjectDesignType>> GetProjectDesignTypeByProjectID(string UnitID);
        Task RemoveByProjectID(string UnitID);
    }
}

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Model;

namespace TimeAPI.Domain.Repositories
{
    public interface IProjectDesignTypeRepository : IRepository<ProjectDesignType, string>
    {
       IEnumerable<ProjectDesignType> GetProjectDesignTypeByUnitID(string UnitID);
       IEnumerable<ProjectDesignType> GetProjectDesignTypeByUnitIDAndProjectTypeID(string UnitID, string ProjectTypeID);

        


        void RemoveByUnitID(string UnitID);


        IEnumerable<ProjectDesignType> GetProjectDesignTypeByProjectID(string UnitID);
        void RemoveByProjectID(string UnitID);

    }
}

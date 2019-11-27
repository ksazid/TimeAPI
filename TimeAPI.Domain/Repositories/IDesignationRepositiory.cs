using System;
using System.Collections.Generic;
using System.Text;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface IDesignationRepositiory : IRepository<Designation, string>
    {
        Designation FindByDesignationName(string dep_name);
        Designation FindByDesignationAlias(string alias);
        IEnumerable<Designation> FindDesignationByDeptID(string DeptID);
    }
}

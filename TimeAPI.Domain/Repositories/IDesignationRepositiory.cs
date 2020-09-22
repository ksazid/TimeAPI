using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface IDesignationRepositiory : IRepositoryAsync<Designation, string>
    {
        Task<Designation> FindByDesignationName(string dep_name);
        Task<Designation> FindByDesignationAlias(string alias);
        Task<IEnumerable<Designation>> FindDesignationByDeptID(string DeptID);
        Task<dynamic> FetchGridDataByDesignationByDeptOrgID(string OrgID);
        Task<dynamic> GetAllDesignationByOrgID(string OrgID);
    }
}

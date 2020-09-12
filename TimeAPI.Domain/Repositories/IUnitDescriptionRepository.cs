using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;
using System.Threading.Tasks;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Model;

namespace TimeAPI.Domain.Repositories
{
    public interface IUnitDescriptionRepository : IRepositoryAsync<UnitDescription, string>
    {
        //dynamic FindByTaskDetailsByEmpID(string empid);

        //void UpdateTaskStatus(CostProjectTask entity);

        //RootEmployeeTask GetAllTaskByEmpID(string empid, string date);

        //RootEmployeeTask GetAllTaskByOrgAndEmpID(string key, string EmpID);
        Task<IEnumerable<UnitDescription>> FetchAllUnitDescriptionByOrgID(string OrgID);
        Task<IEnumerable<UnitDescription>> FetchAllUnitDescriptionExtraByOrgID(string OrgID);


    }
}

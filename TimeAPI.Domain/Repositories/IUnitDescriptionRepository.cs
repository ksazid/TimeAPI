using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Model;

namespace TimeAPI.Domain.Repositories
{
    public interface IUnitDescriptionRepository : IRepository<UnitDescription, string>
    {
        //dynamic FindByTaskDetailsByEmpID(string empid);

        //void UpdateTaskStatus(CostProjectTask entity);

        //RootEmployeeTask GetAllTaskByEmpID(string empid, string date);

        //RootEmployeeTask GetAllTaskByOrgAndEmpID(string key, string EmpID);
        IEnumerable<UnitDescription> FetchAllUnitDescriptionByOrgID(string OrgID);
        IEnumerable<UnitDescription> FetchAllUnitDescriptionExtraByOrgID(string OrgID);

        
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface ITimesheetRepository : IRepositoryAsync<Timesheet, string>
    {
        Task CheckOutByEmpID(Timesheet entity);
        Task<Timesheet> FindTimeSheetByEmpID(string empid, string groupid);
        void RemoveByGroupID(string GroupID);
        Task<dynamic> GetAllTimesheetByOrgID(string OrgID);
        Task<IEnumerable<string>> GetAllEmpByGroupID(string GroupID);

    }
}

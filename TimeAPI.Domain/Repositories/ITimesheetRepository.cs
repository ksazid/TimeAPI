using System;
using System.Collections.Generic;
using System.Text;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface ITimesheetRepository : IRepository<Timesheet, string>
    {
        void CheckOutByEmpID(Timesheet entity);
        Timesheet FindTimeSheetByEmpID(string empid, string groupid);
        void RemoveByGroupID(string GroupID);
        dynamic GetAllTimesheetByOrgID(string OrgID);
        IEnumerable<string> GetAllEmpByGroupID(string GroupID);

    }
}

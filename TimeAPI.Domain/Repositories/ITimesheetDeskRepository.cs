using System;
using System.Collections.Generic;
using System.Text;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface ITimesheetDeskRepository : IRepository<TimesheetDesk, string>
    {
        void CheckOutByEmpID(TimesheetDesk entity);
        TimesheetDesk FindTimeSheetDeskByEmpID(string empid, string groupid);
        void RemoveByGroupID(string GroupID);
        dynamic GetAllTimesheetDeskByOrgID(string OrgID);
        IEnumerable<string> GetAllEmpByGroupID(string GroupID);
    }
}

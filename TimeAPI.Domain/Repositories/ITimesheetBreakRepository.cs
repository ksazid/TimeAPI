using System;
using System.Collections.Generic;
using System.Text;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface ITimesheetBreakRepository : IRepository<TimesheetBreak, string>
    {
        void BreakOutByEmpIDAndGrpID(TimesheetBreak entity);
        TimesheetBreak FindTimeSheetBreakByEmpID(string empid, string groupid);
        void RemoveByGroupID(string GroupID);
        dynamic GetAllTimesheetBreakByOrgID(string OrgID);
        IEnumerable<string> GetAllEmpByGroupID(string GroupID);
        IEnumerable<TimesheetBreak> FindLastTimeSheetBreakByEmpIDAndGrpID(string empid, string groupid);
    }
}

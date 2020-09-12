using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface ITimesheetBreakRepository : IRepositoryAsync<TimesheetBreak, string>
    {
        Task BreakOutByEmpIDAndGrpID(TimesheetBreak entity);
        Task<TimesheetBreak> FindTimeSheetBreakByEmpID(string empid, string groupid);
        Task RemoveByGroupID(string GroupID);
        Task<dynamic> GetAllTimesheetBreakByOrgID(string OrgID);
        Task<IEnumerable<string>> GetAllEmpByGroupID(string GroupID);
        Task<IEnumerable<TimesheetBreak>> FindLastTimeSheetBreakByEmpIDAndGrpID(string empid, string groupid);
    }
}

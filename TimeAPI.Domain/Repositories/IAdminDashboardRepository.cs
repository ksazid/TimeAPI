using System.Collections.Generic;
using System.Threading.Tasks;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Model;

namespace TimeAPI.Domain.Repositories
{
    public interface IAdminDashboardRepository : IRepository<AdminDashboard, string>
    {
         dynamic TotalDefaultEmpCountByOrgID(string OrgID);
         dynamic TotalEmpAbsentCountByOrgIDAndDate(string OrgID, string toDate, string fromDate);
         dynamic TotalEmpAttentedCountByOrgIDAndDate(string OrgID, string toDate, string fromDate);
         dynamic TotalEmpOverTimeCountByOrgIDAndDate(string OrgID, string toDate, string fromDate);
         dynamic TotalEmpLessHoursByOrgIDAndDate(string OrgID, string toDate, string fromDate);
         dynamic TotalLocationExceptionByOrgIDAndDate(string OrgID, string toDate, string fromDate);
         dynamic TotalLocationCheckOutExceptionByOrgIDAndDate(string OrgID, string toDate, string fromDate);
         dynamic GetTimesheetDashboardGridDataByOrgIDAndDate(string OrgID, string toDate, string fromDate);
         dynamic GetTimesheetDashboardFirstCheckInGridDataByOrgIDAndDate(string OrgID, string toDate, string fromDate);
         dynamic GetTimesheetDashboardLastCheckoutGridDataByOrgIDAndDate(string OrgID, string toDate, string fromDate);
         dynamic GetTimesheetDashboardGridAbsentDataByOrgIDAndDate(string OrgID, string toDate, string fromDate);
         dynamic GetCheckOutLocationByGroupID(string GroupID);
         dynamic GetTimesheetActivityByGroupAndDate(string GroupID, string Date);
         dynamic AllProjectRatioByOrgID(string OrgID);
         dynamic GetAllTimesheetRecentActivityList(string OrgID, string toDate, string fromDate);
         dynamic GetAllSingleCheckInEmployeesForHangFireJobs(string OrgID, string toDate, string fromDate);
        IEnumerable<string> GetAllOrgSetupForHangFireJobs();

    }
}

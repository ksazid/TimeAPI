using System.Collections.Generic;
using System.Threading.Tasks;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Model;

namespace TimeAPI.Domain.Repositories
{
    public interface IAdminDashboardRepository : IRepositoryAsync<AdminDashboard, string>
    {
        Task<dynamic> TotalDefaultEmpCountByOrgID(string OrgID);
        Task<dynamic> TotalEmpAbsentCountByOrgIDAndDate(string OrgID, string toDate, string fromDate);
        Task<dynamic> TotalEmpAttentedCountByOrgIDAndDate(string OrgID, string toDate, string fromDate);
        Task<dynamic> TotalEmpOverTimeCountByOrgIDAndDate(string OrgID, string toDate, string fromDate);
        Task<dynamic> TotalEmpLessHoursByOrgIDAndDate(string OrgID, string toDate, string fromDate);
        Task<dynamic> TotalLocationExceptionByOrgIDAndDate(string OrgID, string toDate, string fromDate);
        Task<dynamic> TotalLocationCheckOutExceptionByOrgIDAndDate(string OrgID, string toDate, string fromDate);
        Task<dynamic> GetTimesheetDashboardGridDataByOrgIDAndDate(string OrgID, string toDate, string fromDate);
        Task<dynamic> GetTimesheetDashboardFirstCheckInGridDataByOrgIDAndDate(string OrgID, string toDate, string fromDate);
        Task<dynamic> GetTimesheetDashboardLastCheckoutGridDataByOrgIDAndDate(string OrgID, string toDate, string fromDate);
        Task<dynamic> GetTimesheetDashboardGridAbsentDataByOrgIDAndDate(string OrgID, string toDate, string fromDate);
        Task<dynamic> GetCheckOutLocationByGroupID(string GroupID);
        Task<dynamic> GetTimesheetActivityByGroupAndDate(string GroupID, string Date);
        Task<dynamic> AllProjectRatioByOrgID(string OrgID);
        Task<dynamic> GetAllTimesheetRecentActivityList(string OrgID, string toDate, string fromDate);
        Task<dynamic> GetAllSingleCheckInEmployeesForHangFireJobs(string OrgID, string toDate, string fromDate);
        Task<IEnumerable<string>> GetAllOrgSetupForHangFireJobs();

    }
}

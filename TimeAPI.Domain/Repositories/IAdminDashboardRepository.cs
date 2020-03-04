using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Model;

namespace TimeAPI.Domain.Repositories
{
    public interface IAdminDashboardRepository : IRepository<AdminDashboard, string>
    {
        dynamic TotalDefaultEmpCountByOrgID(string OrgID);
        dynamic TotalEmpAbsentCountByOrgIDAndDate(string OrgID, string toDate, string fromDate);
        dynamic TotalEmpAttentedCountByOrgIDAndDate(string OrgID, string toDate, string fromDate);
        dynamic GetTimesheetDashboardGridDataByOrgIDAndDate(string OrgID, string toDate, string fromDate);
        dynamic GetTimesheetDashboardGridAbsentDataByOrgIDAndDate(string OrgID, string toDate, string fromDate);
        dynamic GetCheckOutLocationByGroupID(string GroupID);
    }
}

using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Model;

namespace TimeAPI.Domain.Repositories
{
    public interface IUserRepository : IRepository<User, string>
    {
        User FindByNormalizedUserName(string normalizedUserName);
        User FindByNormalizedEmail(string normalizedEmail);
        UserDataGroupDataSet GetUserDataGroupByUserID(string EmpID, string Date);
        dynamic TotalEmployeeDashboardDataByOrgID(string OrgID);
        //dynamic TotalEmployeeDashboardDataByOrgID(string OrgID, string toDate, string fromDate);
        dynamic TotalEmployeeAbsentDashboardDataByOrgID(string OrgID, string toDate, string fromDate);
        dynamic GetTimesheetDashboardDataByOrgIDAndDate(string OrgID, string toDate, string fromDate);
        dynamic GetTimesheetDashboardGridDataByOrgIDAndDate(string OrgID, string toDate, string fromDate);
        dynamic GetTimesheetDashboardGridAbsentDataByOrgIDAndDate(string OrgID, string toDate, string fromDate);
    }
}

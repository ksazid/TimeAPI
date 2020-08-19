using System.Collections.Generic;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Model;

namespace TimeAPI.Domain.Repositories
{
    public interface IUserRepository : IRepository<User, string>
    {
        User FindByNormalizedUserName(string normalizedUserName);

        User FindByNormalizedEmail(string normalizedEmail);

        UserDataGroupDataSet GetUserDataGroupByUserID(string EmpID, string Date);

        IEnumerable<RootTimesheetData> GetAllTimesheetByEmpID(string EmpID, string Date);

        IEnumerable<RootTimesheetData> GetAllTimesheetByOrgID(string EmpID, string FromDate, string ToDate);

        dynamic LastCheckinByEmpID(string EmpID, string Date);

        IEnumerable<RootTimesheetData> GetEmployeeTasksTimesheetByEmpID(string EmpID, string FromDate, string ToDate);

        IEnumerable<RootTimesheetData> GetEmployeeTasksTimesheetByOrgID(string EmpID, string FromDate, string ToDate);

        //dynamic TotalEmployeeDashboardDataByOrgID(string OrgID);
        ////dynamic TotalEmployeeDashboardDataByOrgID(string OrgID, string toDate, string fromDate);
        //dynamic TotalEmployeeAbsentDashboardDataByOrgID(string OrgID, string toDate, string fromDate);
        //dynamic GetTimesheetDashboardDataByOrgIDAndDate(string OrgID, string toDate, string fromDate);
        //dynamic GetTimesheetDashboardGridDataByOrgIDAndDate(string OrgID, string toDate, string fromDate);
        //dynamic GetCheckOutLocationByGroupID(string GroupID);
        //dynamic GetTimesheetDashboardGridAbsentDataByOrgIDAndDate(string OrgID, string toDate, string fromDate);
    }
}

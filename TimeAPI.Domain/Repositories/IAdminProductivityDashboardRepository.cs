using System.Collections.Generic;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Model;

namespace TimeAPI.Domain.Repositories
{
    public interface IAdminProductivityDashboardRepository : IRepository<AdminProductivityDashboard, string>
    {
        dynamic ScreenshotByOrgIDAndDate(string OrgID, string StartDate, string EndDate);
        dynamic EmployeeProductivityPerDateByOrgIDAndDate(string OrgID, string StartDate, string EndDate);
        //dynamic DesktopEmployeeProductivityPerDateByOrgIDAndDate(string OrgID, string StartDate, string EndDate);
        dynamic EmployeeProductivityTimeFrequencyByOrgIDAndDate(string OrgID, string StartDate, string EndDate);
        //dynamic EmployeeAppTrackedByOrgIDAndDate(string OrgID, string StartDate, string EndDate);
    }
}

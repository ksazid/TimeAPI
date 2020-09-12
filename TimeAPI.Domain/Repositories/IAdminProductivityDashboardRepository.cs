using System.Collections.Generic;
using System.Threading.Tasks;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Model;

namespace TimeAPI.Domain.Repositories
{
    public interface IAdminProductivityDashboardRepository : IRepositoryAsync<AdminProductivityDashboard, string>
    {
        Task<dynamic> ScreenshotByOrgIDAndDate(string OrgID, string StartDate, string EndDate);
        Task<dynamic> EmployeeProductivityPerDateByOrgIDAndDate(string OrgID, string StartDate, string EndDate);
        //dynamic DesktopEmployeeProductivityPerDateByOrgIDAndDate(string OrgID, string StartDate, string EndDate);
        Task<dynamic> EmployeeProductivityTimeFrequencyByOrgIDAndDate(string OrgID, string StartDate, string EndDate);
        //dynamic EmployeeAppTrackedByOrgIDAndDate(string OrgID, string StartDate, string EndDate);
    }
}

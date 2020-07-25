using System.Collections.Generic;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Model;

namespace TimeAPI.Domain.Repositories
{
    public interface IAdminProductivityDashboardRepository : IRepository<AdminProductivityDashboard, string>
    {
        dynamic ScreenshotByEmpIDAndDate(string EmpID, string StartDate, string EndDate);
        //dynamic DesktopEmployeeAdminProductivityPerDateByEmpIDAndDate(string EmpID, string StartDate, string EndDate);
        //dynamic EmployeeAdminProductivityTimeFrequencyByEmpIDAndDate(string EmpID, string StartDate, string EndDate);
        //dynamic EmployeeAppTrackedByEmpIDAndDate(string EmpID, string StartDate, string EndDate);
    }
}

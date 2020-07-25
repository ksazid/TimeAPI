using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class AdminProductivityDashboardRepository : RepositoryBase, IAdminProductivityDashboardRepository
    {
        public AdminProductivityDashboardRepository(IDbTransaction transaction) : base(transaction)
        {
        }

        #region

        public void Add(AdminProductivityDashboard entity)
        {
            Execute(
                sql: @"",
                param: entity
            );
        }

        public void Update(AdminProductivityDashboard entity)
        {
            Execute(
                sql: @"",
                param: entity
            );
        }

        public IEnumerable<AdminProductivityDashboard> All()
        {
            return Query<AdminProductivityDashboard>(
                sql: ""
            );
        }

        public AdminProductivityDashboard Find(string key)
        {
            return QuerySingleOrDefault<AdminProductivityDashboard>(
                sql: "",
                param: new { key }
            );
        }

        public void Remove(string key)
        {
            Execute(
                sql: "",
                param: new { key }
            );
        }

        #endregion

        public dynamic ScreenshotByEmpIDAndDate(string EmpID, string StartDate, string EndDate)
        {
            return Query<dynamic>(
                 sql: @"SELECT * FROM employee_screenshot
                        WHERE FORMAT(CAST(employee_screenshot.created_date AS date), 'MM/dd/yyyy', 'EN-US')
                    BETWEEN FORMAT(CAST(@StartDate AS DATE), 'MM/dd/yyyy', 'EN-US')
                    AND FORMAT(CAST(@EndDate AS DATE), 'MM/dd/yyyy', 'EN-US') 
                    AND employee_screenshot.emp_id = @EmpID
                    AND employee_screenshot.is_deleted = 0
                    ORDER BY
                    FORMAT(CAST(created_date AS datetime2), N'dd-MMM-yyyy HH:mm:ss', 'EN-US') ASC",
                 param: new { EmpID, StartDate, EndDate }
             );
        }
 

    }

    //public class RootEmployeeProductivityRatio
    //{
    //    public List<EmployeeProductivityTime> employeeProductivityTime { get; set; }
    //    public List<EmployeeIdleTime> employeeIdleTime { get; set; }
    //}

    //public class EmployeeProductivityTime
    //{
    //    public string id { get; set; }
    //    public string start_time { get; set; }
    //    public string end_time { get; set; }
    //    public string productive_ratio { get; set; }
    //    public string ondate { get; set; }
    //    public List<EmployeeProductivityTrackedTime> employeeProductivityTrackedTimes { get; set; }
    //}

    //public class EmployeeIdleTime
    //{
    //    public string id { get; set; }
    //    public string start_time { get; set; }
    //    public string end_time { get; set; }
    //    public string productive_ratio { get; set; }
    //    public string ondate { get; set; }
    //    public List<EmployeeProductivityTrackedTime> employeeProductivityTrackedTimes { get; set; }
    //}

    //public class EmployeeProductivityTrackedTime
    //{
    //    public string app_name { get; set; }
    //    public string time_spend { get; set; }
    //    //public string time_seconds { get; set; }

    //}
}
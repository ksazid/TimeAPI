using System;
using System.Collections.Generic;
using System.Text;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Model
{
    public class UserDataGroupDataSet
    {
        public User User { get; set; }
        public Employee Employee { get; set; }
        public IEnumerable<Organization> Organization { get; set; }
        public IEnumerable<RootTimesheetData> Timesheet { get; set; }

    }

    public class RootTimesheetData
    {
        public IEnumerable<TimesheetDataModel> timesheetDataModels { get; set; }
        public IEnumerable<TimesheetAdministrativeDataModel> TimesheetAdministrativeDataModel { get; set; }
        public IEnumerable<TimesheetProjectCategoryDataModel> TimesheetProjectCategoryDataModel { get; set; }
        public IEnumerable<TimesheetTeamDataModel> TimesheetTeamDataModel { get; set; }
    }

    public class TimesheetDataModel
    {
        public string id { get; set; }
        public string emp_name { get; set; }
        public string groupid { get; set; }
        public string teamid { get; set; }
        public string team_name { get; set; }
        public string ondate { get; set; }
        public string check_in { get; set; }
        public string check_out { get; set; }
        public bool is_checkout { get; set; }
        public string total_hrs { get; set; }
        public string created_date { get; set; }
        public string createdby { get; set; }
        public string modified_date { get; set; }
        public string modifiedby { get; set; }
        public bool is_deleted { get; set; }
       
    }

    public class TimesheetProjectCategoryDataModel
    {
        public string id { get; set; }
        public string groupid { get; set; }
        public string project_name { get; set; }
        public string system_name { get; set; }
    }

    public class TimesheetAdministrativeDataModel
    {
        public string id { get; set; }
        public string administrative_id { get; set; }
        public string administrative_name { get; set; }

    }

    public class TimesheetTeamDataModel
    {
        public string id { get; set; }
        public string teamid { get; set; }
        public string team_name { get; set; }

    }
}

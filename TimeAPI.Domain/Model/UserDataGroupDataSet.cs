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
        public Subscription Subscription { get; set; }
        public IEnumerable<Organization> Organization { get; set; }
        //public IEnumerable<RootTimesheetData> Timesheet { get; set; }

    }

    public class RootTimesheetData
    {
        public IEnumerable<TimesheetDataModel> TimesheetDataModels { get; set; }
        public IEnumerable<TimesheetTeamDataModel> TimesheetTeamDataModels { get; set; }
        //public IEnumerable<TimesheetAdministrativeDataModel> TimesheetAdministrativeDataModel { get; set; }
        public TimesheetProjectCategoryDataModel TimesheetProjectCategoryDataModel { get; set; }
        public TimesheetSearchLocationViewModel TimesheetSearchLocationViewModel { get; set; }
        public IEnumerable<TimesheetCurrentLocationViewModel> TimesheetCurrentLocationViewModels { get; set; }
        public IEnumerable<string> Members { get; set; }
    }

    public class TimesheetDataModel
    {
        public string id { get; set; }
        public string emp_id { get; set; }
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

        public List<ViewLogDataModel> viewLogDataModels { get; set; }

    }

    public class TimesheetProjectCategoryDataModel
    {
        public string category_id { get; set; }
        public string groupid { get; set; }
        public string project_type { get; set; }
        public string project_or_comp_id { get; set; }
        public string project_or_comp_name { get; set; }
        public string project_or_comp_type { get; set; }
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

    public class TimesheetTeamMembersDataModel
    {
        public string emp_id { get; set; }
    }

    public class TimesheetSearchLocationViewModel
    {
        public string id { get; set; }
        public string groupid { get; set; }
        public string manual_address { get; set; }
        public string geo_address { get; set; }
        public string formatted_address { get; set; }
        public string lat { get; set; }
        public string lang { get; set; }
        public string street_number { get; set; }
        public string route { get; set; }
        public string locality { get; set; }
        public string administrative_area_level_2 { get; set; }
        public string administrative_area_level_1 { get; set; }
        public string postal_code { get; set; }
        public string country { get; set; }
        public bool is_office { get; set; }
        public bool is_manual { get; set; }

    }

    public class TimesheetCurrentLocationViewModel
    {
        public string id { get; set; }
        public string groupid { get; set; }
        public string formatted_address { get; set; }
        public string lat { get; set; }
        public string lang { get; set; }
        public string street_number { get; set; }
        public string route { get; set; }
        public string locality { get; set; }
        public string administrative_area_level_2 { get; set; }
        public string administrative_area_level_1 { get; set; }
        public string postal_code { get; set; }
        public string country { get; set; }
        public bool is_checkout { get; set; }
    }

    public class TimesheetAbsent
    {
        public string id { get; set; }
        public string full_name { get; set; }
        public string workemail { get; set; }
        public string emp_code { get; set; }
        public string mobile { get; set; }
        public string employee_status_name { get; set; }
        public string employee_type_name { get; set; }
        public string role_name { get; set; }
        public string dep_name { get; set; }
        public string department_id { get; set; }
        public string designation_name { get; set; }
        public string ondate { get; set; }
        public int rowno { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TimeAPI.API.Models.TimesheetViewModels
{
    public class TimesheetPostViewModel
    {
        public List<string> team_member_empid { get; set; }
        public List<string> teamid { get; set; }
        public string check_in { get; set; }
        public string createdby { get; set; }
        //public TimesheetAdministrativeViewModel TimesheetAdministrativeViewModel { get; set; }
        public TimesheetCategoryViewModel TimesheetCategoryViewModel { get; set; }
        public TimesheetSearchLocationViewModel TimesheetSearchLocationViewModel { get; set; }
        public TimesheetCurrentLocationViewModel TimesheetCurrentLocationViewModel { get; set; }
    }

    public class TimesheetViewModel
    {
        public string id { get; set; }
        public List<string> team_member_empid { get; set; }
        public string groupid { get; set; }
        public List<string> teamid { get; set; }
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
        //public TimesheetAdministrativeViewModel TimesheetAdministrativeViewModel { get; set; }
        public TimesheetCategoryViewModel TimesheetCategoryViewModel { get; set; }
        public TimesheetSearchLocationViewModel TimesheetSearchLocationViewModel { get; set; }
        public TimesheetCurrentLocationViewModel TimesheetCurrentLocationViewModel { get; set; }

    }

    public class TimesheetAdministrativeViewModel
    {
        public List<string> administrative_id { get; set; }
    }

    public class TimesheetCategoryViewModel
    {
        public string project_category_id { get; set; }
        public string project_or_comp_id { get; set; }
    }

    public class TimesheetSearchLocationViewModel
    {
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
    }

    //for checkout 
    public class TimesheetCheckoutViewModel
    {
        public List<string> team_member_empid { get; set; }
        public string groupid { get; set; }
        public string check_out { get; set; }
        public string modifiedby { get; set; }
        public TimesheetCurrentLocationViewModel TimesheetCurrentLocationViewModel { get; set; }
    }
}

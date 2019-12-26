using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TimeAPI.API.Models.TimesheetViewModels
{
    public class TimesheetViewModel
    {
        public string id { get; set; }
        public List<string> team_member_empid { get; set; }
        public string groupid { get; set; }
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

        //For TimesheetActivity
        public string project_category_type_id { get; set; }
        public string system_id { get; set; }

        //FOr TimesheetAdministrative
        public List<string> team_administrative { get; set; }

    }
}

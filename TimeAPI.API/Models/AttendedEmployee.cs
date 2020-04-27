using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TimeAPI.API.Models
{
    public class AttendedEmployee
    {
        public string rowno { get; set; }
        public string project_category_type { get; set; }
        public string project_or_comp_name { get; set; }
        public string timesheet_id { get; set; }
        public string groupid { get; set; }
        public bool is_checkout { get; set; }
        public string employee_id { get; set; }
        public string workhour { get; set; }
        public string full_name { get; set; }
        public string check_in { get; set; }
        public string check_out { get; set; }
        public string total_hrs { get; set; }
        public string lat { get; set; }
        public string lang { get; set; }
        public string ondate { get; set; }
    }
}

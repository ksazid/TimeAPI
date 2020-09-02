using System;
using System.Collections.Generic;
using System.Text;

namespace TimeAPI.Domain.Entities
{
    public class TimesheetActivity
    {
        public string id { get; set; } 
        public string groupid { get; set; }
        public string project_id { get; set; }
        public string milestone_id { get; set; }
        public string milestone_name { get; set; }
        public string task_id { get; set; }
        public string task_name { get; set; }
        public string remarks { get; set; }
        public string worked_percent { get; set; }
        public string ondate { get; set; }
        public string start_time { get; set; }
        public string end_time { get; set; }
        public string total_hrs { get; set; }
        public bool is_billable { get; set; }
        public string created_date { get; set; }
        public string createdby { get; set; }
        public string modified_date { get; set; }
        public string modifiedby { get; set; }
        public bool is_deleted { get; set; }

    }
}

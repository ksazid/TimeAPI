using System;
using System.Collections.Generic;
using System.Text;

namespace TimeAPI.Domain.Entities
{
    public class TimesheetProjectCategory
    {
        public string id { get; set; }
        //public string timesheet_id { get; set; }
        public string groupid { get; set; }
        public string project_category_id { get; set; }
        public string project_or_comp_id { get; set; }
        public bool is_office { get; set; }
        public bool is_other { get; set; }
        public string created_date { get; set; }
        public string createdby { get; set; }
        public string modified_date { get; set; }
        public string modifiedby { get; set; }
        public bool is_deleted { get; set; }

    }
}

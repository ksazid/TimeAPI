using System;
using System.Collections.Generic;
using System.Text;

namespace TimeAPI.Domain.Entities
{
    public class TimesheetBreak
    {
        public string id { get; set; }
        public string orgid { get; set; }
        public string empid { get; set; }
        public string groupid { get; set; }
        public string ondate { get; set; }
        public string break_in { get; set; }
        public string break_out { get; set; }
        public bool is_breakout { get; set; }
        public string total_hrs { get; set; }
        public string created_date { get; set; }
        public string createdby { get; set; }
        public string modified_date { get; set; }
        public string modifiedby { get; set; }
        public bool is_deleted { get; set; }

    }
}

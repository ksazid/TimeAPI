using System;
using System.Collections.Generic;
using System.Text;

namespace TimeAPI.Domain.Entities
{
    public class EmployeeLeaveLog
    {
        public string id { get; set; }
        public string emp_leave_id { get; set; }
        public string emp_id { get; set; }
        public string leave_type { get; set; }
        public string start_date { get; set; }
        public string end_date { get; set; }
        public string no_of_days { get; set; }
        public string ondate { get; set; }
        public string from_user { get; set; }
        public string to_user { get; set; }
        public string created_date { get; set; }
        public string createdby { get; set; }
        public string modified_date { get; set; }
        public string modifiedby { get; set; }
        public bool is_deleted { get; set; }
    }
}

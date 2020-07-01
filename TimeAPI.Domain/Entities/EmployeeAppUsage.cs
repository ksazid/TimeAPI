using System;
using System.Collections.Generic;
using System.Text;

namespace TimeAPI.Domain.Entities
{
    public class EmployeeAppUsage
    {
        public string id { get; set; }
        public string org_id { get; set; }
        public string emp_id { get; set; }
        public string start_time { get; set; }
        public string end_time { get; set; }
        public string idle_time { get; set; }
        public string ondate { get; set; }
        public string created_date { get; set; }
        public string createdby { get; set; }
        public string modified_date { get; set; }
        public string modifiedby { get; set; }
        public bool is_deleted { get; set; }
    }
}

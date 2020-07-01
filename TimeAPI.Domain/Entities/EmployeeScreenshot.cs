using System;
using System.Collections.Generic;
using System.Text;

namespace TimeAPI.Domain.Entities
{
    public class EmployeeScreenshot
    {
        public string id { get; set; }
        public string org_id { get; set; }
        public string emp_id { get; set; }
        public string img_name { get; set; }
        public string img_url { get; set; }
        public string ondate { get; set; }
        public string created_date { get; set; }
        public string createdby { get; set; }
        public string modified_date { get; set; }
        public string modifiedby { get; set; }
        public bool is_deleted { get; set; }
    }
}

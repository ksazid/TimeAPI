using System;
using System.Collections.Generic;
using System.Text;

namespace TimeAPI.Domain.Entities
{
    public class EmployeeStatus
    {
        public string id { get; set; }
        public string employee_status_name { get; set; }
        public string employee_status_desc { get; set; }
        public string created_date { get; set; }
        public string createdby { get; set; }
        public string modified_date { get; set; }
        public string modifiedby { get; set; }
        public bool is_deleted { get; set; }
    }
}

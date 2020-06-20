using System;
using System.Collections.Generic;
using System.Text;

namespace TimeAPI.Domain.Entities
{
    public class EmployeeLeave
    {
        public string id { get; set; }
        public string org_id { get; set; }
        public string emp_id { get; set; }
        public string leave_setup_id { get; set; }
        public string leave_start_date { get; set; }
        public string leave_end_date { get; set; }
        public string leave_days { get; set; }
        public string ondate_applied { get; set; }
        public string approver_emp_id { get; set; }
        public bool is_approved { get; set; }
        public string approve_start_date { get; set; }
        public string approve_end_date { get; set; }
        public string approved_days { get; set; }
        public string ondate_approved { get; set; }
        public string emp_notes { get; set; }
        public string approver_notes { get; set; }
        public string leave_status_id { get; set; }
        public string created_date { get; set; }
        public string createdby { get; set; }
        public string modified_date { get; set; }
        public string modifiedby { get; set; }
        public bool is_deleted { get; set; }
    }
}

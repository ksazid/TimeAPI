using System;
using System.Collections.Generic;
using System.Text;

namespace TimeAPI.Domain.Entities
{
    public class Tasks
    {
        public string id { get; set; }
        public string empid { get; set; }
        public string project_id { get; set; }
        public string activtity_id { get; set; }
        public string task_name { get; set; }
        public string task_desc { get; set; }
        public string priority_id { get; set; }
        public string status_id { get; set; }
        public string assigned_empid { get; set; }
        public string due_date { get; set; }
        public bool is_approver { get; set; }
        public string is_approver_id { get; set; }
        public string is_approved { get; set; }
        public string unit { get; set; }
        public string qty { get; set; }
        public string created_date { get; set; }
        public string createdby { get; set; }
        public string modified_date { get; set; }
        public string modifiedby { get; set; }
        public bool is_deleted { get; set; }
        public IEnumerable<TaskTeamMember> employees { get; set; }

    }
}

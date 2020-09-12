using System;
using System.Collections.Generic;
using System.Text;

namespace TimeAPI.Domain.Entities
{
    public class SubTasks
    {
        public string id { get; set; }
        public string task_id { get; set; }
        public string sub_task_name { get; set; }
        public string sub_task_desc { get; set; }
        public string priority_id { get; set; }
        public string status_id { get; set; }
        public string lead_id { get; set; }
        public string due_date { get; set; }
        public bool is_approver { get; set; }
        public string is_approver_id { get; set; }
        public string is_approved { get; set; }
        public string unit_type { get; set; }
        public string hrs_per_unit { get; set; }
        public string qty { get; set; }
        //public bool is_local_activity { get; set; }
        public string created_date { get; set; }
        public string createdby { get; set; }
        public string modified_date { get; set; }
        public string modifiedby { get; set; }
        public bool is_deleted { get; set; }
        public IEnumerable<TaskTeamMember> employees { get; set; }

    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace TimeAPI.Domain.Entities
{
    public class CostProjectTask
    {
        public string id { get; set; }
        public string project_id { get; set; }
        public bool is_selected { get; set; }
        public string milestone_id { get; set; }
        public string task_name { get; set; }
        public string total_unit { get; set; }
        public string default_unit_hours { get; set; }
        public string unit { get; set; }
        public string qty { get; set; }
        public string notes { get; set; }
        public string discount_amount { get; set; }
        public string total_cost_amount { get; set; }
        public string created_date { get; set; }
        public string createdby { get; set; }
        public string modified_date { get; set; }
        public string modifiedby { get; set; }
        public bool is_deleted { get; set; }

        public IEnumerable<TaskTeamMember> employees { get; set; }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TimeAPI.API.Models.TaskTemplateViewModels
{
    public class TaskTemplateViewModel
    {
        public string id { get; set; }
        public string org_id { get; set; }
        public string template_name { get; set; }
        public string milestone_id { get; set; }
        public string task_name { get; set; }
        public string task_desc { get; set; }
        public string priority_id { get; set; }
        public string assignee_emp_id { get; set; }
        public string due_date { get; set; }
        public bool is_approve_req { get; set; }
        public string approve_emp_id { get; set; }
        public string created_date { get; set; }
        public string createdby { get; set; }
        public string modified_date { get; set; }
        public string modifiedby { get; set; }
        public bool is_deleted { get; set; }
    }
}

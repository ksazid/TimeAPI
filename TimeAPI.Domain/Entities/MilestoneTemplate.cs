using System;
using System.Collections.Generic;
using System.Text;

namespace TimeAPI.Domain.Entities
{
    public class MilestoneTemplate
    {
        public string id { get; set; }
        public string org_id { get; set; }
        public string template_name { get; set; }
        public string milestone_name { get; set; }
        public string milestone_desc { get; set; }
        public string start_date { get; set; }
        public string end_date { get; set; }
        public bool is_approve_req { get; set; }
        public string approve_emp_id { get; set; }
        public string created_date { get; set; }
        public string createdby { get; set; }
        public string modified_date { get; set; }
        public string modifiedby { get; set; }
        public bool is_deleted { get; set; }
    }
}

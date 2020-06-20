using System;
using System.Collections.Generic;
using System.Text;

namespace TimeAPI.Domain.Entities
{
    public class CostProjectMilestone
    {
        public string id { get; set; }
        public string org_id { get; set; }
        public string project_id { get; set; }
        public string milestone_name { get; set; }
        public string alias_name { get; set; }
        public string created_date { get; set; }
        public string createdby { get; set; }
        public string modified_date { get; set; }
        public string modifiedby { get; set; }
        public bool is_deleted { get; set; }
        public List<CostProjectTask> CostProjectTask { get; set; }
    }
}

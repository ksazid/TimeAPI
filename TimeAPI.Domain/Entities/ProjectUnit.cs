using System;
using System.Collections.Generic;
using System.Text;

namespace TimeAPI.Domain.Entities
{
    public class ProjectUnit
    {
        public string id { get; set; }
        public string org_id { get; set; }
        public string project_id { get; set; }
        public string unit_id { get; set; }
        public string unit_name { get; set; }
        public string no_of_unit { get; set; }
        public string unit_qty { get; set; }
        public string note { get; set; }
        public string created_date { get; set; }
        public string createdby { get; set; }
        public string modified_date { get; set; }
        public string modifiedby { get; set; }
        public bool is_deleted { get; set; }
        public List<string> ProjectDesignType_ID { get; set; }
        public List<ProjectTags> ProjectTags { get; set; }
    }
}

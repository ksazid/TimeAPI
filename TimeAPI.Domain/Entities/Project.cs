using System;
using System.Collections.Generic;
using System.Text;

namespace TimeAPI.Domain.Entities
{
    public class Project
    {
        public string id { get; set; }
        public string user_id { get; set; }
        public string org_id { get; set; }
        public string project_name { get; set; }
        public string project_desc { get; set; }
        public string start_date { get; set; }
        public string end_date { get; set; }
        public string completed_date { get; set; }
        public string project_status { get; set; }
        public bool is_private { get; set; }
        public bool is_public { get; set; }
        public bool is_inactive { get; set; }
        public string createdby { get; set; }
        public string created_date { get; set; }
        public string modified_date { get; set; }
        public string modifiedby { get; set; }
        public bool is_deleted { get; set; }
    }
}

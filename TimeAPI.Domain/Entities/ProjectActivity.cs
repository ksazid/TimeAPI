﻿using System;
using System.Collections.Generic;
using System.Text;

namespace TimeAPI.Domain.Entities
{
    public class ProjectActivity
    {
        public string id { get; set; }
        public string project_id { get; set; }
        public string activity_name { get; set; }
        public string activity_desc { get; set; }
        public string unit { get; set; }
        public string qty { get; set; }
        public string start_date { get; set; }
        public string end_date { get; set; }
        public bool is_approve_req { get; set; }
        public string approved_id { get; set; }
        public bool is_approved { get; set; }
        public string status_id { get; set; }
        public string status_name { get; set; }
        public string created_date { get; set; }
        public string createdby { get; set; }
        public string modified_date { get; set; }
        public string modifiedby { get; set; }
        public bool is_deleted { get; set; }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Text;

namespace TimeAPI.Domain.Entities
{
    public class Department
    {
        public string id { get; set; }
        public string org_id { get; set; }
        public string depart_lead_empid { get; set; }
        public string dep_name { get; set; }
        public string alias { get; set; }
        public string created_date { get; set; }
        public string createdby { get; set; }
        public string modified_date { get; set; }
        public string modifiedby { get; set; }
        public bool is_deleted { get; set; }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Text;

namespace TimeAPI.Domain.Entities
{
    public class LeaveType
    {
        public string id { get; set; }
        public string org_id { get; set; }
        public string leave_type_name { get; set; }
        public string created_date { get; set; }
        public string createdby { get; set; }
        public string modified_date { get; set; }
        public string modifiedby { get; set; }
        public bool is_deleted { get; set; }
    }
}

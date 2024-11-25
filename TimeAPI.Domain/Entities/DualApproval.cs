﻿using System;
using System.Collections.Generic;
using System.Text;

namespace TimeAPI.Domain.Entities
{
    public class DualApproval
    {
        public string id { get; set; }
        public string entity_id { get; set; }
        public string approver1_empid { get; set; }
        public string approver2_empid { get; set; }
        public string createdby { get; set; }
        public string modified_date { get; set; }
        public string modifiedby { get; set; }
        public bool is_deleted { get; set; }
    }
}

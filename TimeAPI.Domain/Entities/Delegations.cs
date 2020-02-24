using System;
using System.Collections.Generic;
using System.Text;

namespace TimeAPI.Domain.Entities
{
    public class Delegations
    {
        public string id { get; set; }
        public string org_id { get; set; }
        public string delegation_name { get; set; }
        public string delegator { get; set; }
        public string delegations_desc { get; set; }
        public string created_date { get; set; }
        public string createdby { get; set; }
        public string modified_date { get; set; }
        public string modifiedby { get; set; }
        public bool is_deleted { get; set; }
    }
}

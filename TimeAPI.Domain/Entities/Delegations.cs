using System;
using System.Collections.Generic;
using System.Text;

namespace TimeAPI.Domain.Entities
{
    public class Delegations
    {
        public string id { get; set; }
        public string org_id { get; set; }
        public string delegator { get; set; }
        public bool is_type_temporary { get; set; }
        public bool is_type_permanent { get; set; }
        public bool is_notify_delegator_and_delegatee { get; set; }
        public bool is_notify_delegatee { get; set; }
        public string delegations_desc { get; set; }
        public string created_date { get; set; }
        public string createdby { get; set; }
        public string modified_date { get; set; }
        public string modifiedby { get; set; }
        public bool is_deleted { get; set; }
    }
}

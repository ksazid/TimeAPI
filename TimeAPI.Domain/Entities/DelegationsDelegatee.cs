using System;
using System.Collections.Generic;
using System.Text;

namespace TimeAPI.Domain.Entities
{
    public class DelegationsDelegatee
    {
        public string id { get; set; }
        public string delegator_id { get; set; }
        public string delegatee_id { get; set; }
        public bool is_type_temporary { get; set; }
        public string expires_on { get; set; }
        public bool is_type_permanent { get; set; }
        public bool is_notify_delegator_and_delegatee { get; set; }
        public bool is_notify_delegatee { get; set; }
        public string role_id { get; set; }
        public string created_date { get; set; }
        public string createdby { get; set; }
        public string modified_date { get; set; }
        public string modifiedby { get; set; }
        public bool is_deleted { get; set; }
    }
}

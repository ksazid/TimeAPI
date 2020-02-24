using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TimeAPI.API.Models.DelegationsViewModels
{
    public class DelegationsViewModel
    {
        public string id { get; set; }
        public string org_id { get; set; }
        public string delegation_name { get; set; }
        public string delegator { get; set; }
        public List<string> delegatee_emp_id { get; set; }
        public bool is_type_temporary { get; set; }
        public string expires_on { get; set; }
        public bool is_type_permanent { get; set; }
        public bool is_notify_delegator_and_delegatee { get; set; }
        public bool is_notify_delegatee { get; set; }
        public string delegations_desc { get; set; }
        public string role_id { get; set; }
        public string created_date { get; set; }
        public string createdby { get; set; }
        public string modified_date { get; set; }
        public string modifiedby { get; set; }
        public bool is_deleted { get; set; }


        

    }
}

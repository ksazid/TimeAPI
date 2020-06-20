using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TimeAPI.API.Models.LeaveSetupViewModels
{
    public class LeaveSetupViewModel
    {
        public string id { get; set; }
        public string org_id { get; set; }
        public string leave_name { get; set; }
        public string leave_type_id { get; set; }
        public string max_leave_days { get; set; }
        public string created_date { get; set; }
        public string createdby { get; set; }
        public string modified_date { get; set; }
        public string modifiedby { get; set; }
        public bool is_deleted { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TimeAPI.API.Models.ProjectActivityViewModels
{
    public class ProjectActivityViewModel
    {
        public string id { get; set; }
        public string project_id { get; set; }
        public string activity_name { get; set; }
        public string activity_desc { get; set; }
        public string unit { get; set; }
        public string qty { get; set; }
        public bool is_approve_req { get; set; }
        public bool approved_id { get; set; }
        public bool is_approved { get; set; }
        public string status_id { get; set; }
        public string created_date { get; set; }
        public string createdby { get; set; }
        public string modified_date { get; set; }
        public string modifiedby { get; set; }
        public bool is_deleted { get; set; }
    }

    public class ProjectActivityStatusUpdateViewModel
    {
        public string status_id { get; set; }
        public string modified_date { get; set; }
        public string modifiedby { get; set; }
    }
}
    
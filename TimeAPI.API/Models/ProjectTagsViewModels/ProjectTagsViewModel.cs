using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TimeAPI.API.Models.ProjectTagsViewModels
{
    public class ProjectTagsViewModel
    {
        public string id { get; set; }
        public string project_id { get; set; }
        public string unit_id { get; set; }
        public string tags { get; set; }
        public string created_date { get; set; }
        public string createdby { get; set; }
        public string modified_date { get; set; }
        public string modifiedby { get; set; }
        public bool is_deleted { get; set; }
    }
}
    
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using TimeAPI.Domain.Entities;

namespace TimeAPI.API.Models.LeadProjectViewModels
{
    public class LeadProjectViewModel
    {
        public string id { get; set; }
        public string lead_id { get; set; }
        public string project_prefix { get; set; }
        public string project_name { get; set; }
        public string design_type_id { get; set; }
        public string project_type_id { get; set; }
        public string packages_id { get; set; }
        public string created_date { get; set; }
        public string createdby { get; set; }
        public string modified_date { get; set; }
        public string modifiedby { get; set; }
        public bool is_deleted { get; set; }
    }
}

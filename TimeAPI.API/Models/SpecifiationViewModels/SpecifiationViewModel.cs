using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TimeAPI.Domain.Entities;

namespace TimeAPI.API.Models.SpecifiationViewModels
{
    public class SpecifiationViewModel
    {
        public string id { get; set; }
        public string org_id { get; set; }
        public string specification_name { get; set; }
        public string specification_qty { get; set; }
        public string created_date { get; set; }
        public string createdby { get; set; }
        public string modified_date { get; set; }
        public string modifiedby { get; set; }
        public bool is_deleted { get; set; }
    }
 
}

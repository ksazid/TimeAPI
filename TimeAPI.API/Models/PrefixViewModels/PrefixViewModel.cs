using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TimeAPI.API.Models.PrefixViewModels
{
    public class PrefixViewModel
    {
        public string id { get; set; }
        public string org_id { get; set; }
        public string type { get; set; }
        public string prefix_ext { get; set; }
        public string prefix_name { get; set; }
        public string prefix_for { get; set; }
        public bool is_manual_allowed { get; set; }
        public bool is_revised { get; set; }
        public string created_date { get; set; }
        public string createdby { get; set; }
        public string modified_date { get; set; }
        public string modifiedby { get; set; }
        public bool is_deleted { get; set; }
    }
}

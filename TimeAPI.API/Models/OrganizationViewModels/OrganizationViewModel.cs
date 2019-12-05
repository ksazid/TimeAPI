using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TimeAPI.API.Models.OrganizationViewModels
{
    public class OrganizationViewModel
    {
        public string org_id { get; set; }
        public string user_id { get; set; }
        public string org_name { get; set; }
        public string type { get; set; }
        public string summary { get; set; }
        public string img_url { get; set; }
        public string img_name { get; set; }
        public string country_id { get; set; }
        public string adr1 { get; set; }
        public string adr2 { get; set; }
        public string city { get; set; }
        public string primary_cont_name { get; set; }
        public string primary_cont_type { get; set; }
        public string time_zone_id { get; set; }
        public string created_date { get; set; }
        public string createdby { get; set; }
        public string modified_date { get; set; }
        public string modifiedby { get; set; }
        public bool is_deleted { get; set; }
    }
}

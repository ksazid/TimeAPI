using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using TimeAPI.Domain.Entities;

namespace TimeAPI.API.Models.LeadViewModels
{
    public class LeadViewModel
    {
        public string id { get; set; }
        public string org_id { get; set; }
        public string lead_company_id { get; set; }
        public string lead_owner_emp_id { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string lead_source_id { get; set; }
        public string lead_status_id { get; set; }
        public string annual_revenue { get; set; }
        public string rating_id { get; set; }
        public string industry_id { get; set; }
        public string no_of_employee { get; set; }
        public string email { get; set; }
        public string website { get; set; }
        public string adr_1 { get; set; }
        public string adr_2 { get; set; }
        public string city { get; set; }
        public string country_id { get; set; }
        public string created_date { get; set; }
        public string createdby { get; set; }
        public string modified_date { get; set; }
        public string modifiedby { get; set; }
        public bool is_deleted { get; set; }
        public List<EntityContact> EntityContact { get; set; }
        public LeadProject LeadProject { get; set; }

    }
}

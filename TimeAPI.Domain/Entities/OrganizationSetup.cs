using System;
using System.Collections.Generic;
using System.Text;

namespace TimeAPI.Domain.Entities
{
    public class OrganizationSetup
    {
        public string id { get; set; }
        public string org_id { get; set; }
        public string country { get; set; }
        public string fiscal_year { get; set; }
        public List<WeekendHours> weekends { get; set; }
        public string working_hrs { get; set; }
        public string date_format { get; set; }
        public string currency { get; set; }
        public string time_zome { get; set; }
        public string created_date { get; set; }
        public string createdby { get; set; }
        public string modified_date { get; set; }
        public string modifiedby { get; set; }
        public bool is_deleted { get; set; }
    }
}

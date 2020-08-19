using System;
using System.Collections.Generic;
using System.Text;

namespace TimeAPI.Domain.Entities
{
    public class LeadDeal
    {
        public string id { get; set; }
        public string lead_id { get; set; }
        public string deal_prefix { get; set; }
        public string deal_name { get; set; }
        public string deal_type_id { get; set; }
        public string est_amount { get; set; }
        public string stage_id { get; set; }
        public string contact_role_id { get; set; }
        public string est_closing_date { get; set; }
        public string basic_cost { get; set; }
        public bool is_manual { get; set; }
        public string remarks { get; set; }
        public string created_date { get; set; }
        public string createdby { get; set; }
        public string modified_date { get; set; }
        public string modifiedby { get; set; }
        public bool is_deleted { get; set; }
    }

    public class LeadDealUpdate
    {
        public string lead_id { get; set; }
        public string est_amount { get; set; }
        public bool is_manual { get; set; }
        public string basic_cost { get; set; }
        public string remarks { get; set; }
        public string modifiedby { get; set; }
    }
}

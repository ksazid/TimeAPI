using System;
using System.Collections.Generic;
using System.Text;

namespace TimeAPI.Domain.Entities
{
    public class Subscription
    {
        public string id { get; set; }
        public string user_id { get; set; }
        public string org_id { get; set; }
        public string api_key { get; set; }
        public string subscription_start_date { get; set; }
        public string subscription_end_date { get; set; }
        public string on_date_subscribed { get; set; }
        public string current_plan_id { get; set; }
        public string offer_id { get; set; }
        public string offer_start_date { get; set; }
        public string offer_end_date { get; set; }
        public bool is_trial { get; set; }
        public bool is_subscibe_after_trial { get; set; }
        public bool is_active { get; set; }
        public string created_date { get; set; }
        public string createdby { get; set; }
        public string modified_date { get; set; }
        public string modifiedby { get; set; }
        public bool is_deleted { get; set; }
    }
}

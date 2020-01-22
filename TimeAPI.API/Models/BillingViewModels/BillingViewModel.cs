using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TimeAPI.API.Models.BillingViewModels
{
    public class BillingViewModel
    {
        public string id { get; set; }
        public string user_id { get; set; }
        public string user_email { get; set; }
        public string current_plan_id { get; set; }
        public string billing_cycle { get; set; }
        public string total_user { get; set; }
        public string total_cost { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string card_no { get; set; }
        public string expire_month { get; set; }
        public string expire_year { get; set; }
        public string cvv { get; set; }
        public string adr1 { get; set; }
        public string zip { get; set; }
        public string state { get; set; }
        public string country { get; set; }
        public string created_date { get; set; }
        public string createdby { get; set; }
        public string modified_date { get; set; }
        public string modifiedby { get; set; }
        public bool is_deleted { get; set; }
    }
}

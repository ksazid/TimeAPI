using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using TimeAPI.Domain.Entities;

namespace TimeAPI.API.Models.QuotationViewModels
{
    public class QuotationViewModel
    {
        public string id { get; set; }
        public string org_id { get; set; }
        public string lead_id { get; set; }
        public string quotation_prefix { get; set; }
        public string quotation_date { get; set; }
        public string customer_id { get; set; }
        public string project_name { get; set; }
        public string quotation_subject { get; set; }
        public string quotation_body { get; set; }
        public string warranty_id { get; set; }
        public string validity { get; set; }
        public string payment_id { get; set; }
        public string mode_of_payment_id { get; set; }
        public string no_of_days { get; set; }
        public string exclusion_id { get; set; }
        public string remarks { get; set; }
        public string tax_id { get; set; }
        public string stage_id { get; set; }
        public string created_date { get; set; }
        public string createdby { get; set; }
        public string modified_date { get; set; }
        public string modifiedby { get; set; }
        public bool is_deleted { get; set; }
    }


     public class QuotationStageViewModel
    {
        public string id { get; set; }
        public string stage_id { get; set; }
        public string modifiedby { get; set; }
    }
}

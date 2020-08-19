using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TimeAPI.API.Models.PaymentViewModels
{
    public class PaymentViewModel
    {
        public string id { get; set; }
        public string org_id { get; set; }
        public string payment_name { get; set; }
        public string payment_desc { get; set; }
        public string created_date { get; set; }
        public string createdby { get; set; }
        public string modified_date { get; set; }
        public string modifiedby { get; set; }
        public bool is_deleted { get; set; }
    }
}

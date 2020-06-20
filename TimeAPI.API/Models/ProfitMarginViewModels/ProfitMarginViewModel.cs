using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TimeAPI.Domain.Entities;

namespace TimeAPI.API.Models.ProfitMarginViewModels
{
    public class ProfitMarginViewModel
    {
        public string id { get; set; }
        public string org_id { get; set; }
        public string profit_margin { get; set; }
        public string created_date { get; set; }
        public string createdby { get; set; }
        public string modified_date { get; set; }
        public string modifiedby { get; set; }
        public bool is_deleted { get; set; }
    }
 
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using TimeAPI.Domain.Entities;

namespace TimeAPI.API.Models.CustomerViewModels
{
    public class CustomerViewModel
    {
        public string id { get; set; }
        public string org_id { get; set; }
        public string cst_name { get; set; }
        public string cst_type { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public string adr { get; set; }
        public string street { get; set; }
        public string country { get; set; }
        public string city { get; set; }
        public string created_date { get; set; }
        public string createdby { get; set; }
        public string modified_date { get; set; }
        public string modifiedby { get; set; }
        public bool is_deleted { get; set; }
        public EntityContact EntityContact { get; set; }

    }
}

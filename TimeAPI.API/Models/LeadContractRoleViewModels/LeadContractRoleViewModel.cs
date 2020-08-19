using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using TimeAPI.Domain.Entities;

namespace TimeAPI.API.Models.LeadContractRoleViewModels
{
    public class LeadContractRoleViewModel
    {
        public string id { get; set; }
        public string org_id { get; set; }
        public string contact_role_name { get; set; }
        public string contact_role_desc { get; set; }
        public string created_date { get; set; }
        public string createdby { get; set; }
        public string modified_date { get; set; }
        public string modifiedby { get; set; }
        public bool is_deleted { get; set; }
    }
}

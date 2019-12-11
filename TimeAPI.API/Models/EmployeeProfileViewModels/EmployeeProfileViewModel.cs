using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace TimeAPI.API.Models.EmployeeProfileViewModels
{
    public class EmployeeProfileViewModel
    {
        public string id { get; set; }
        public string user_id { get; set; }
        public string img_name { get; set; }
        public string img_url { get; set; }
        public string created_date { get; set; }
        public string createdby { get; set; }
        public string modified_date { get; set; }
        public string modifiedby { get; set; }
        public bool is_deleted { get; set; }

    }
}

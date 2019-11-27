using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TimeAPI.API.Models.EmployeeViewModels
{
    public class EmployeeViewModel
    {
        public string id { get; set; }
        public string org_id { get; set; }
        public string user_id { get; set; }
        public string deptid { get; set; }
        public string full_name { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string alias { get; set; }
        public string gender { get; set; }
        public string emp_status { get; set; }
        public string emp_type { get; set; }
        public string imgurl { get; set; }
        public string workemail { get; set; }
        public string emp_code { get; set; }
        public string role { get; set; }
        public string designation { get; set; }
        public string dob { get; set; }
        public string joined_date { get; set; }
        public string phone { get; set; }
        public string mobile { get; set; }
        public string email { get; set; }
        public string summary { get; set; }
        public string created_date { get; set; }

        [Required (ErrorMessage = "Please enter current user full name")]
        public string createdby { get; set; }
        public string modified_date { get; set; }
        public string modifiedby { get; set; }
        public bool is_deleted { get; set; }
        public bool is_admin { get; set; }
    }
}

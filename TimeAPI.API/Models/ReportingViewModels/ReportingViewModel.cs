using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TimeAPI.API.Models.ReportingViewModels
{
    public class ReportingViewModel
    {
        public string id { get; set; }
        public string empid { get; set; }
        public string report_emp_id { get; set; }
        public string created_date { get; set; }

        [Required(ErrorMessage = "Please enter current user full name")]
        public string createdby { get; set; }
        public string modified_date { get; set; }
        public string modifiedby { get; set; }
        public bool is_deleted { get; set; }
    }
}

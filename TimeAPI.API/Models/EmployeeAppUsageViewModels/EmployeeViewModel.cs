using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace TimeAPI.API.Models.EmployeeAppUsageViewModels
{
    public class EmployeeAppUsageViewModel
    {
        public string id { get; set; }
        public string org_id { get; set; }
        public string emp_id { get; set; }
        public List<AppUsedViewModel> AppUsedViewModel { get; set; }
        public string start_time { get; set; }
        public string end_time { get; set; }
        public string idle_time { get; set; }
        public string ondate { get; set; }
        public string created_date { get; set; }
        public string createdby { get; set; }
        public string modified_date { get; set; }
        public string modifiedby { get; set; }
        public bool is_deleted { get; set; }
    }

    public class AppUsedViewModel
    {
        public string app_name { get; set; }
        public string time_spend { get; set; }
        public string icon { get; set; }
    }

}

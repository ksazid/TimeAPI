using System;
using System.Collections.Generic;
using System.Text;

namespace TimeAPI.Domain.Entities
{
    public class EmployeeAppTracked
    {
        public string id { get; set; }
        public string emp_app_usage_id { get; set; }
        public string emp_id { get; set; }
        public string app_name { get; set; }
        public string time_spend { get; set; }
        public string icon { get; set; }
        public bool is_productive { get; set; }
        public bool is_unproductive { get; set; }
        public bool is_neutral { get; set; }
        public string created_date { get; set; }
        public string createdby { get; set; }
        public string modified_date { get; set; }
        public string modifiedby { get; set; }
        public bool is_deleted { get; set; }
    }
}

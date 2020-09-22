using System;
using System.Collections.Generic;
using System.Text;

namespace TimeAPI.Domain.Entities
{
    public class Workforce
    {
        public string team_members { get; set; }
        public string working { get; set; }
        public string total_productive_percent { get; set; }
        public string total_productive_time { get; set; }
        public List<EmployeeWorkforce> EmployeeWorkforce { get; set; }

    }

    public class EmployeeWorkforce
    {
        public string org_id { get; set; }
        public string emp_id { get; set; }
        public string full_name { get; set; }
        public string deptartment_name { get; set; }
        public string designation_name { get; set; }
        public string workemail { get; set; }
        public string productive_time { get; set; }
        public string desktop_time { get; set; }
        public string arrival_time { get; set; }
        public string left_time { get; set; }
        public string time_at_work { get; set; }
        public string productive_percent { get; set; }

    }
}

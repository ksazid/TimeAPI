using System;
using System.Collections.Generic;
using System.Text;

namespace TimeAPI.Domain.Entities
{
    public class WeekendHours
    {
        public string id { get; set; }
        public string org_id { get; set; }
        public string offworkdays { get; set; }
        public string start_time { get; set; }
        public string end_time { get; set; }
        public string created_date { get; set; }
        public string createdby { get; set; }
        public string modified_date { get; set; }
        public string modifiedby { get; set; }
        public bool is_deleted { get; set; }
    }
}

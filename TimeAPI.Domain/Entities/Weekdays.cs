using System;
using System.Collections.Generic;
using System.Text;

namespace TimeAPI.Domain.Entities
{
    public class Weekdays
    {
        public string id { get; set; }
        public string org_id { get; set; }
        public string day_name { get; set; }
        public string from_time { get; set; }
        public string to_time { get; set; }
        public bool is_off { get; set; }
        public string created_date { get; set; }
        public string createdby { get; set; }
        public string modified_date { get; set; }
        public string modifiedby { get; set; }
        public bool is_deleted { get; set; }
    }
}

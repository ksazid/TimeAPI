using System;
using System.Collections.Generic;
using System.Text;

namespace TimeAPI.Domain.Entities
{
    public class EntityCall
    {
        public string id { get; set; }
        public string entity_id { get; set; }
        public string contact_id { get; set; }
        public string subject { get; set; }
        public string call_purpose { get; set; }
        public bool is_current_call { get; set; }
        public bool is_completed_call { get; set; }
        public bool is_schedule_call { get; set; }
        public string start_time { get; set; }
        public string end_time { get; set; }
        public string call_desc { get; set; }
        public string call_result { get; set; }
        public string host { get; set; }
        public string created_date { get; set; }
        public string createdby { get; set; }
        public string modified_date { get; set; }
        public string modifiedby { get; set; }
        public bool is_deleted { get; set; }
    }
}

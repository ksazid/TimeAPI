using System;
using System.Collections.Generic;
using System.Text;

namespace TimeAPI.Domain.Entities
{
    public class EntityContact
    {
        public string id { get; set; }
        public string entity_id { get; set; }
        public string name { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string relationship { get; set; }
        public string department { get; set; }
        public string designation { get; set; }
        public string position { get; set; }
        public string phone { get; set; }
        public string mobile { get; set; }
        public string email { get; set; }
        public string adr_1 { get; set; }
        public string adr_2 { get; set; }
        public string city { get; set; }
        public string country { get; set; }
        public bool is_primary { get; set; }
        public string note { get; set; }
        public string createdby { get; set; }
        public string created_date { get; set; }
        public string modified_date { get; set; }
        public string modifiedby { get; set; }
        public bool is_deleted { get; set; }
    }
}

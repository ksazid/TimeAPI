using System;
using System.Collections.Generic;
using System.Text;

namespace TimeAPI.Domain.Entities
{
    public class LocationException
    {
        public string id { get; set; }
        public string group_id { get; set; }
        public string checkin_lat { get; set; }
        public string checkin_lang { get; set; }
        public bool is_chkin_inrange { get; set; }
        public string checkout_lat { get; set; }
        public string checkout_lang { get; set; }
        public bool is_chkout_inrange { get; set; }
        public bool is_checkout { get; set; }
        public string created_date { get; set; }
        public string createdby { get; set; }
        public string modified_date { get; set; }
        public string modifiedby { get; set; }
        public bool is_deleted { get; set; }
    }
}

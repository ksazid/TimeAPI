using System;
using System.Collections.Generic;
using System.Text;

namespace TimeAPI.Domain.Entities
{
    public class Location
    {
        public string id { get; set; }
        public string groupid { get; set; }
        public string geo_address { get; set; }
        public string formatted_address { get; set; }
        public string lat { get; set; }
        public string lang { get; set; }
        public string street_number { get; set; }
        public string route { get; set; }
        public string locality { get; set; }
        public string administrative_area_level_2 { get; set; }
        public string administrative_area_level_1 { get; set; }
        public string postal_code { get; set; }
        public string country { get; set; }
        public string created_date { get; set; }
        public string createdby { get; set; }
        public string modified_date { get; set; }
        public string modifiedby { get; set; }
        public bool is_deleted { get; set; }
        public bool is_checkout { get; set; }

    }
}

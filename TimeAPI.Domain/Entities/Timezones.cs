using System;
using System.Collections.Generic;
using System.Text;

namespace TimeAPI.Domain.Entities
{
    public class Timezones
    {
        public int id { get; set; }
        public string timezone_location { get; set; }
        public string gmt { get; set; }
        public string offset { get; set; }
    }
}

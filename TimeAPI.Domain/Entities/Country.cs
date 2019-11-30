using System;
using System.Collections.Generic;
using System.Text;

namespace TimeAPI.Domain.Entities
{
    public class Country
    {
        public int id { get; set; }
        public string iso { get; set; }
        public string name { get; set; }
        public string iso3 { get; set; }
        public string numcode { get; set; }
        public string phonecode { get; set; }

    }
}

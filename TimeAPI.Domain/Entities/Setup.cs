using System;
using System.Collections.Generic;
using System.Text;

namespace TimeAPI.Domain.Entities
{
   public class Setup
    {
        public IEnumerable<Country> Country { get; set; }
        public IEnumerable<Timezones> Timezones { get; set; }
    }
}

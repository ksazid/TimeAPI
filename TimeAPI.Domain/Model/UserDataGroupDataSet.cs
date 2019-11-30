using System;
using System.Collections.Generic;
using System.Text;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Model
{
    public class UserDataGroupDataSet
    {
        public User User { get; set; }
        public Employee Employee { get; set; }
        public IEnumerable<Organization> Organization { get; set; }
    }
}

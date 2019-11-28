using System;
using System.Collections.Generic;
using System.Text;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Model
{
    public class UserDataGroupDataSet
    {
        public User _User { get; set; }
        public Employee _Employee { get; set; }
        public IEnumerable<Organization> _Organization { get; set; }
    }
}

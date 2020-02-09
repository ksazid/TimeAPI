using System;
using System.Collections.Generic;
using System.Text;

namespace TimeAPI.Domain.Entities
{
    public class Administrative
    {
        public string id { get; set; }
        public string dept_id { get; set; }
        public string org_id { get; set; }
        public string administrative_name { get; set; }
        public string summary { get; set; }
        public string created_date { get; set; }
        public string createdby { get; set; }
        public string modified_date { get; set; }
        public string modifiedby { get; set; }
        public bool is_deleted { get; set; }
    }

    public class RootObject
    {
      public List<RootDepartmentObject> rootDepartmentObjects { get; set; }
    }

    public class RootDepartmentObject
    {
        public string id { get; set; }
        public string dept_name { get; set; }
        public List<AdministrativeDropDown> administratives { get; set; }
    }


    public class AdministrativeDropDown
    {
        public string id { get; set; }
        public string text { get; set; }
    }
}

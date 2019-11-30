using System;
using System.Collections.Generic;
using System.Text;

namespace TimeAPI.Domain.Entities
{
    public class Task
    {
        public string id { get; set; } 
        public string task_name { get; set; }
        public string task_desc { get; set; }
        public string priority { get; set; }
        public string status { get; set; }
        public string assigned_empid { get; set; }
        public string due_date { get; set; }
        public string created_date { get; set; }
        
        public string createdby { get; set; }
        public string modified_date { get; set; }
        public string modifiedby { get; set; }
        public bool is_deleted { get; set; }
    }
}

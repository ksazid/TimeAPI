using System;
using System.Collections.Generic;
using System.Text;

namespace TimeAPI.Domain.Model
{
    public class RootEmployeeTask
    {
        public IEnumerable<EmployeeTasks> EmployeeTasks { get; set; }
        public IEnumerable<EmployeeTasks> AssignedEmployeeTasks { get; set; }

    }

    public class EmployeeTasks
    {
        public string id { get; set; }
        public string empid { get; set; }
        public string task_name { get; set; }
        public string task_desc { get; set; }
        public string priority { get; set; }
        public string status { get; set; }
        public string assigned_to { get; set; }
        public string due_date { get; set; }
        public string created_date { get; set; }

    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace TimeAPI.Domain.Model
{
    public class RootEmployeeTask
    {
        public IEnumerable<EmployeeTasks> EmployeeTasks { get; set; }
        public IEnumerable<EmployeeTasks> AssignedEmployeeTasks { get; set; }
        public IEnumerable<EmployeeTasks> OverDueTasks { get; set; }
    }

    public class EmployeeTasks
    {
        public string id { get; set; }
        public string empid { get; set; }
        public string project_id { get; set; }
        public string milestone_id { get; set; }
        public string task_name { get; set; }
        public string project_name { get; set; }
        public string milestone_name { get; set; }
        public string task_desc { get; set; }
        public string priority { get; set; }
        public string status_id { get; set; }
        public string status_name { get; set; }
        public string assigned_to { get; set; }
        public string assigned_to_name { get; set; }
        public string is_approver { get; set; }
        public string is_approver_id { get; set; }
        public string approver_name { get; set; }
        public string due_date { get; set; }
        public string created_date { get; set; }

    }

    public class ViewLogDataModel
    {
        public string total_time { get; set; }
        public string members { get; set; }
        public string project_type { get; set; }
        public string project_name { get; set; }
        public string milestone_name { get; set; }
        public string task_name { get; set; }
        public string remarks { get; set; }
        public string total_hrs { get; set; }
        public string is_billable { get; set; }
        public string ondate { get; set; }
        public string start_time { get; set; }
        public string end_time { get; set; }
        public string groupid { get; set; }
    }
}

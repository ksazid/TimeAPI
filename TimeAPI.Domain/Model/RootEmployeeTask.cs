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
        public string sub_task_id { get; set; }
        public string assigned_empid { get; set; }
        public string assigned_name { get; set; }
        public string project_id { get; set; }
        public string milestone_id { get; set; }
        public string subtask_name { get; set; }
        public string task_name { get; set; }
        public string project_name { get; set; }
        public string milestone_name { get; set; }
        public string task_desc { get; set; }
        public string priority_name { get; set; }
        public string status_id { get; set; }
        public string status_name { get; set; }
        public string lead_id { get; set; }
        public string lead_name { get; set; }
        public string is_approver { get; set; }
        public string is_approver_id { get; set; }
        public string is_local_activity { get; set; }
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
        public string worked_percent { get; set; }
        public string ondate { get; set; }

        private string _start_time;
        public string start_time
        {
            get { return _start_time; }
            set { _start_time = value.TrimStart(); }
        }
        private string _end_time;
        public string end_time
        {
            get { return _end_time; }
            set { _end_time = value.TrimStart(); }
        }
        public string groupid { get; set; }
    }
}

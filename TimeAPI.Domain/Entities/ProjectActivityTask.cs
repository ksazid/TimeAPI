using System;
using System.Collections.Generic;
using System.Text;

namespace TimeAPI.Domain.Entities
{
    public class ProjectActivityTask
    {
        public string id { get; set; }
        public string project_id { get; set; }
        public string activity_id { get; set; }
        public string task_id { get; set; }
        public string created_date { get; set; }
        public string createdby { get; set; }
        public string modified_date { get; set; }
        public string modifiedby { get; set; }
        public bool is_deleted { get; set; }
         
    }


    public class ProjectActivityTaskEntityViewModel
    {
        public string id { get; set; }
        public string project_id { get; set; }
        public string activity_id { get; set; }
        public string task_id { get; set; }
        public string task_name { get; set; }
        public string due_date { get; set; }
        public string activity_name { get; set; }
        public string assigned_empid { get; set; }
        public string assignedto { get; set; }
        public string priority_name { get; set; }
        public string status_name { get; set; }
        public string status_id { get; set; }
        public string created_date { get; set; }
        public string createdby { get; set; }
        public string modified_date { get; set; }
        public string modifiedby { get; set; }
        public bool is_deleted { get; set; }
        public List<TaskTeamMember> TaskTeamMember { get; set; }
    }
}

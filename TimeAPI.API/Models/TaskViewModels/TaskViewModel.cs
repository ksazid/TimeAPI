﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TimeAPI.Domain.Entities;

namespace TimeAPI.API.Models.TaskViewModels
{
    public class TaskViewModel
    {
        public string id { get; set; }
        public string empid { get; set; }
        public string task_name { get; set; }
        public string task_desc { get; set; }
        public string priority_id { get; set; }
        public string status_id { get; set; }
        public string assigned_empid { get; set; }
        public string due_date { get; set; }
        public string created_date { get; set; }
        public string createdby { get; set; }
        public string modified_date { get; set; }
        public string modifiedby { get; set; }
        public bool is_deleted { get; set; }
        public bool is_approver { get; set; }
        public string is_approver_id { get; set; }
        public List<TaskTeamMember> employees { get; set; }
    }

    public class Employees
    {
        public IEnumerable<string> empid { get; set; }
    }
    public class TaskUpdateStatusViewModel
    {
        public string id { get; set; }
        public string status_id { get; set; }
        public string modified_date { get; set; }
        public string modifiedby { get; set; }
    }
}

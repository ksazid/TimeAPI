﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TimeAPI.Domain.Entities;

namespace TimeAPI.API.Models.TaskViewModels
{
    public class ProjectActivityTaskViewModel
    {
        public string id { get; set; }
        public string project_id { get; set; }
        public string activtity_id { get; set; }
        public string empid { get; set; }
        public string task_name { get; set; }
        public string task_desc { get; set; }
        public string priority_id { get; set; }
        public string status_id { get; set; }
        public string assigned_empid { get; set; }
        public string due_date { get; set; }
        public string createdby { get; set; }
        public bool is_approver { get; set; }
        public string is_approver_id { get; set; }
        public bool is_local_activity { get; set; }
        public string unit { get; set; }
        public string qty { get; set; }
        public IEnumerable<TaskTeamMember> employees { get; set; }
    }


    public class AssignEmployeeTaskViewModel
    {
        public string id { get; set; }
        public string empid { get; set; }
        public string createdby { get; set; }
    }
}
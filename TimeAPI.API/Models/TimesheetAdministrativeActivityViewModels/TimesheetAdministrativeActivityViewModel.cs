﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TimeAPI.API.Models.TimesheetActivityViewModels
{
    public class TimesheetAdministrativeActivityViewModel
    {
        public string id { get; set; }
        public string administrative_id { get; set; }
        public string groupid { get; set; }
        public string purpose { get; set; }
        public string remarks { get; set; }
        public string ondate { get; set; }
        public string start_time { get; set; }
        public string end_time { get; set; }
        public string created_date { get; set; }
        public string createdby { get; set; }
        public string modified_date { get; set; }
        public string modifiedby { get; set; }
        public bool is_deleted { get; set; }

    }
}
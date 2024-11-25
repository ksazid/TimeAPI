﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TimeAPI.API.Models.PlanFeatureViewModels
{
    public class PlanFeatureViewModel
    {
        public string id { get; set; }
        public string plan_id { get; set; }
        public string feature_name { get; set; }
        public string feature_desc { get; set; }
        public string created_date { get; set; }
        public string createdby { get; set; }
        public string modified_date { get; set; }
        public string modifiedby { get; set; }
        public bool is_deleted { get; set; }
    }
}

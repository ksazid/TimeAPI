using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TimeAPI.API.Models.TimesheetActivityFileViewModels
{
    public class TimesheetActivityFileViewModel
    {
        public string id { get; set; }
        public string task_id { get; set; }
        public string subtask_id { get; set; }
        public string file_name { get; set; }
        public string file_type { get; set; }
        public string ondate { get; set; }
        public string created_date { get; set; }
        public string createdby { get; set; }
        public string modified_date { get; set; }
        public string modifiedby { get; set; }
        public bool is_deleted { get; set; }
    }
}

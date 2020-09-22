using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TimeAPI.API.Models.EntityMeetingViewModels
{
    public class EntityMeetingViewModel
    {
        public string id { get; set; }
        public string org_id { get; set; }
        public string emp_id { get; set; }
        public string entity_id { get; set; }
        public string meeting_name { get; set; }
        public string location { get; set; }
        public string desc { get; set; }
        public string start_time { get; set; }
        public string end_time { get; set; }
        public string host { get; set; }
        public string host_name { get; set; }
        public List<string> participant_id { get; set; }
        public string created_date { get; set; }
        public string createdby { get; set; }
        public string modified_date { get; set; }
        public string modifiedby { get; set; }
        public bool is_deleted { get; set; }
    }
}

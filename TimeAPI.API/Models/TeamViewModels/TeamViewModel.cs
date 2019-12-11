using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TimeAPI.API.Models.StatusViewModels
{
    public class TeamViewModel
    {
        public string id { get; set; }
        public string org_id { get; set; }
        public IEnumerable<string> teammember_empids { get; set; }
        public string team_name { get; set; }
        public string team_desc { get; set; }
        public string team_by { get; set; }
        public string team_department_id { get; set; }
        public string created_date { get; set; }
        public string createdby { get; set; }
        public string modified_date { get; set; }
        public string modifiedby { get; set; }
        public bool is_deleted { get; set; }
    }
}

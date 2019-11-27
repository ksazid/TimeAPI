using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TimeAPI.API.Models.DepartmentViewModels
{
    public class DepartmentViewModels
    {
        public DepartmentViewModels()
        {
            id = Guid.NewGuid().ToString();
            created_date = DateTime.Now.ToString();
        }

        public string id { get; set; }

        [Required(ErrorMessage = "enter department lead")]
        public string depart_lead_empid { get; set; }

        [Required(ErrorMessage = "enter department name")]
        public string dep_name { get; set; }

        [Required(ErrorMessage = "enter department alias")]
        public string alias { get; set; }
        public string created_date { get; set; }

        [Required(ErrorMessage = "enter current user")]
        public string createdby { get; set; }
        public string modified_date { get; set; }
        public string modifiedby { get; set; }
        public bool is_deleted { get; set; }

    }
}

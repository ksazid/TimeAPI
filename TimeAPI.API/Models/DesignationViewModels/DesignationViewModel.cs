using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TimeAPI.API.Models.DesignationViewModels
{
    public class DesignationViewModel
    {
        public string id { get; set; }
        public string dep_id { get; set; }
        public string designation_name { get; set; }
        public string alias { get; set; }
        public string created_date { get; set; }

        [Required(ErrorMessage = "enter current user")]
        public string createdby { get; set; }
        public string modified_date { get; set; }
        public string modifiedby { get; set; }
        public bool is_deleted { get; set; }
    }
}

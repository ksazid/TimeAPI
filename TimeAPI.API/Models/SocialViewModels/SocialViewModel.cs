using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TimeAPI.API.Models.SocialViewModels
{
    public class SocialViewModel
    {
        public string id { get; set; }
        public string empid { get; set; }
        public string social_media_name { get; set; }
        public string url { get; set; }
        public string created_date { get; set; }

        [Required(ErrorMessage = "Please enter current user full name")]
        public string createdby { get; set; }
        public string modified_date { get; set; }
        public string modifiedby { get; set; }
        public bool is_deleted { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TimeAPI.Domain.Entities;

namespace TimeAPI.API.Models.ProjectViewModels
{
    public class ProjectViewModel
    {
        public string id { get; set; }
        public string user_id { get; set; }
        public string org_id { get; set; }
        public string cst_id { get; set; }
        public string project_type_id { get; set; }
        public string project_name { get; set; }
        public string project_desc { get; set; }
        public string start_date { get; set; }
        public string end_date { get; set; }
        public string completed_date { get; set; }
        public string project_status_id { get; set; }
        public string project_prefix { get; set; }
        public bool is_private { get; set; }
        public bool is_public { get; set; }
        public bool is_inactive { get; set; }
        public string createdby { get; set; }
        public EntityContact EntityContact { get; set; }
        public EntityLocation EntityLocation { get; set; }
    }


    public class ProjectDetailViewModel
    {
        public string id { get; set; }
        public string user_id { get; set; }
        public string org_id { get; set; }
        public string cst_id { get; set; }
        public string project_name { get; set; }
        public string project_desc { get; set; }
        public string start_date { get; set; }
        public string end_date { get; set; }
        public string completed_date { get; set; }
        public string project_status_id { get; set; }
        public string project_prefix { get; set; }
        public bool is_private { get; set; }
        public bool is_public { get; set; }
        public bool is_inactive { get; set; }
        public string createdby { get; set; }
        public EntityContact EntityContact { get; set; }
        public EntityLocation EntityLocation { get; set; }
        public Customer EntityCustomer { get; set; }
    }


    public class ProjectStatusModel
    {
        public string id { get; set; }
        public string project_status_id { get; set; }
        public string modifiedby { get; set; }
        //public EntityContact EntityContact { get; set; }
        //public EntityLocation EntityLocation { get; set; }
    }

    public class ProjectCustomerViewModel
    {
        public string cst_id { get; set; }
        public string project_id { get; set; }
        public string createdby { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using TimeAPI.API.Models.EntityLocationViewModels;

namespace TimeAPI.API.Models.OrganizationViewModels
{
    public class OrganizationViewModel
    {
        public string org_id { get; set; }
        public string user_id { get; set; }
        public string org_name { get; set; }
        public string type { get; set; }
        public string summary { get; set; }
        public string img_url { get; set; }
        public string img_name { get; set; }
        public string country_id { get; set; }
        public string adr1 { get; set; }
        public string adr2 { get; set; }
        public string city { get; set; }
        public string primary_cont_name { get; set; }
        public string primary_cont_type { get; set; }
        public string time_zone_id { get; set; }
        public string created_date { get; set; }
        public string createdby { get; set; }
        public string modified_date { get; set; }
        public string modifiedby { get; set; }
        public bool is_deleted { get; set; }

        public EntityLocationViewModel EntityLocationViewModel { get; set; }

    }


    //public class EntityLocationViewModel
    //{
    //    public string geo_address { get; set; }
    //    public string formatted_address { get; set; }
    //    public string lat { get; set; }
    //    public string lang { get; set; }
    //    public string street_number { get; set; }
    //    public string route { get; set; }
    //    public string locality { get; set; }
    //    public string administrative_area_level_2 { get; set; }
    //    public string administrative_area_level_1 { get; set; }
    //    public string postal_code { get; set; }
    //    public string country { get; set; }
    //}


    public class OrganizationBranchViewModel
    {
        public string parent_org_id { get; set; }
        public string org_id { get; set; }
        public string user_id { get; set; }
        public string org_name { get; set; }
        public string type { get; set; }
        public string summary { get; set; }
        public string img_url { get; set; }
        public string img_name { get; set; }
        public string country_id { get; set; }
        public string adr1 { get; set; }
        public string adr2 { get; set; }
        public string city { get; set; }
        public string primary_cont_name { get; set; }
        public string primary_cont_type { get; set; }
        public string time_zone_id { get; set; }
        public string created_date { get; set; }
        public string createdby { get; set; }
        public string modified_date { get; set; }
        public string modifiedby { get; set; }
        public bool is_deleted { get; set; }

        public EntityLocationViewModel EntityLocationViewModel { get; set; }

    }
}

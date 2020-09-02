using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TimeAPI.Domain.Entities;

namespace TimeAPI.API.Models.CostProjectViewModels
{
    public class CostProjectViewModel
    {
        public string id { get; set; }
        public string user_id { get; set; }
        public string org_id { get; set; }
        //public Customer customer { get; set; }
        public string cst_id { get; set; }
        public string package_id { get; set; }
        public string project_type_id { get; set; }
        public string project_name { get; set; }

        //public string project_desc { get; set; }
        //public string start_date { get; set; }
        //public string end_date { get; set; }
        //public string completed_date { get; set; }
        //public string project_status_id { get; set; }
        public string project_prefix { get; set; }
        //public bool is_site_visit { get; set; }
        public string no_of_floors { get; set; }
        public string total_unit { get; set; }
        public string plot_size { get; set; }
        public string plot_size_unit { get; set; }
        public string buildup_area { get; set; }
        public string buildup_area_unit { get; set; }
        public string discount_amount { get; set; }
        public string profit_margin_amount { get; set; }
        public string createdby { get; set; }
        public List<string> TypeOfDesign { get; set; }
        public List<ProjectUnit> ProjectUnit { get; set; }
        public List<EntityContact> EntityContact { get; set; }
        public List<CostProjectMilestone> CostProjectMilestone { get; set; }
        //public EntityLocation EntityLocation { get; set; }
    }

    

    public class UpdateCostProjectViewModel
    {
        public string id { get; set; }
        public string discount_amount { get; set; }
        public string profit_margin_amount { get; set; }
        public string createdby { get; set; }

    }



    public class UpdateCostProjectFinalValueViewModel
    {
        public string id { get; set; }
        public string total_hours { get; set; }
        public string gross_total_amount { get; set; }
        public string profit_margin_amount { get; set; }
        public string discount_amount { get; set; }
        public string total_amount { get; set; }
        public string vat_amount { get; set; }
        public string net_total_amount { get; set; }
        public string createdby { get; set; }

    }


    public class UpdateCostProjectIsQuotationViewModel
    {
        public string id { get; set; }
        public bool is_quotation { get; set; }
        public string modifiedby { get; set; }

    }


    public class CostProjectResponseViewModel
    {
        public string id { get; set; }
        public string user_id { get; set; }
        public string org_id { get; set; }
        public string cst_id { get; set; }
        public string customer_name { get; set; }
        public string package_id { get; set; }
        public string package_name { get; set; }
        public string project_type_id { get; set; }
        public string project_type_name { get; set; }
        public string project_name { get; set; }

        //public string project_desc { get; set; }
        //public string start_date { get; set; }
        //public string end_date { get; set; }
        //public string completed_date { get; set; }
        //public string project_status_id { get; set; }
        public string project_prefix { get; set; }
        public string no_of_floors { get; set; }
        public string total_unit { get; set; }
        public string plot_size { get; set; }
        public string plot_size_unit { get; set; }
        public string buildup_area { get; set; }
        public string buildup_area_unit { get; set; }
        public string discount_amount { get; set; }
        public string profit_margin_amount { get; set; }
        public string createdby { get; set; }
        public List<string> TypeOfDesign { get; set; }
        public List<ProjectUnit> ProjectUnit { get; set; }
        public List<ProjectUnit> ProjectUnitExtra { get; set; }
        public List<EntityContact> EntityContact { get; set; }
        public List<CostProjectMilestone> CostProjectMilestone { get; set; }
        //public EntityLocation EntityLocation { get; set; }
    }

    public class CostProjectDetailViewModel
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
        //public EntityLocation EntityLocation { get; set; }
        public Customer EntityCustomer { get; set; }
    }

    public class CostProjectStatusModel
    {
        public string id { get; set; }
        public string project_status_id { get; set; }
        public string project_prefix { get; set; }
        public string modifiedby { get; set; }
        public string start_date { get; set; }
        public string end_date { get; set; }

        //public EntityContact EntityContact { get; set; }
        public EntityLocation EntityLocation { get; set; }
    }

    public class CostProjectCustomerViewModel
    {
        public string cst_id { get; set; }
        public string project_id { get; set; }
        public string createdby { get; set; }
    }
}

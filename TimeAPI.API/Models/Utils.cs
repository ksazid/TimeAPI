﻿using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using TimeAPI.API.Models.PlanFeatureViewModels;
using TimeAPI.API.Models.PlanPriceViewModels;
using TimeAPI.API.Models.PlanViewModels;
using TimeAPI.Domain.Entities;

namespace TimeAPI.API.Models
{

    public class LowercaseContractResolver : DefaultContractResolver
    {
        protected override string ResolvePropertyName(string propertyName)
        {
            return propertyName.ToLower();
        }
    }

    public class LowercaseNamingStrategy : NamingStrategy
    {
        protected override string ResolvePropertyName(string name)
        {
            return name.ToLowerInvariant();
        }
    }
    public class Utils
    {
        public string ID { get; set; }
    }

    public class UtilsEntityAndCstID
    {
        public string EntityID { get; set; }
        public string CstID { get; set; }
    }

    public class UtilsGroupIDAndDate
    {
        public string ID { get; set; }
        public string Date { get; set; }
    }

    public class UtilsMilestoneIDAndOrgID
    {
        public string ID { get; set; }
        public string OrgID { get; set; }
    }

    public class UtilsCustomerNameAndEmail
    {
        public string CustomerName { get; set; }
        public string Email { get; set; }
    }

    public class RootPlanDetails
    {
        public List<RootPlanViewModel> PlanViewModel { get; set; }
    }
    public class RootPlanViewModel
    {
        public string id { get; set; }
        public string plan_name { get; set; }
        public string plan_desc { get; set; }
        public List<PlanPrice> _PlanPriceViewModel { get; set; }
        public List<PlanFeature> _PlanFeatureViewModel { get; set; }
    }

    public class UtilsOrgID
    {
        public string OrgID { get; set; }
    }

    public class UtilsOrgIDAndType
    {
        public string OrgID { get; set; }
        public string Type { get; set; }
    }

    public class UtilsOrgIDAndPrefix
    {
        public string OrgID { get; set; }
        public string Prefix { get; set; }
    }

    public class UtilsOrgIDAndDate
    {
        public string OrgID { get; set; }
        public string fromDate { get; set; }
        public string toDate { get; set; }
    }

    public class UtilsOrgAndEmpID
    {
        public string OrgID { get; set; }
        public string EmpID { get; set; }
    }

    public class UtilsEmpIDAndGrpID
    {
        public string GrpID { get; set; }
        public string EmpID { get; set; }
    }

    public class UtilsName
    {
        public string FullName { get; set; }
    }

    public class UtilsCode
    {
        public string Code { get; set; }
    }

    public class UtilsRole
    {
        public string Role { get; set; }
    }

    //public class UtilsOrg
    //{
    //    public string OrgID { get; set; }
    //}

    public class UtilsAlias
    {
        public string Alias { get; set; }
    }

    public class oDataTable
    {
        public DataTable ToDataTable(IEnumerable<dynamic> items)
        {
            if (items == null) return null;
            var data = items.ToArray();
            if (data.Length == 0) return null;

            var dt = new DataTable();
            foreach (var pair in ((IDictionary<string, object>)data[0]))
            {
                dt.Columns.Add(pair.Key, (pair.Value ?? string.Empty).GetType());
            }
            foreach (var d in data)
            {
                dt.Rows.Add(((IDictionary<string, object>)d).Values.ToArray());
            }
            return dt;
        }

    }

    public class UtilPhoneResult
    {
        public string Code { get; set; }
        public string PhoneNumber { get; set; }
        public string Message { get; set; }
    }

    public class UtilsGroupID
    {
        public string GroupID { get; set; }
        public string AdministrativeID { get; set; }
    }

    public class UtilsGroupIDAndProjectID
    {
        public string GroupID { get; set; }
        public string ProjectID { get; set; }
        public string Date { get; set; }
    }

    public class UtilsEmpIDAndDate
    {
        public string EmpID { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
    }

    public class UtilsIDAndDate
    {
        public string ID { get; set; }
        public string Date { get; set; }
    }

    public class UtilsDateAndOrgID
    {
        public string OrgID { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
    }

    public class UtilsEmpIDAndDateWithoutWeek
    {
        public string EmpID { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public bool? isWeek { get; set; }
    }



    public class UtilPhone
    {
        public string PhoneNumber { get; set; }
    }

    public class Message
    {
        public string Type { get; set; }
        public string Payload { get; set; }
    }

    public class UtilDepartmentChecked
    {
        public string org_id { get; set; }
        public string createdby { get; set; }
        public bool is_accounts { get; set; }
        public bool is_administrative { get; set; }
        public bool is_advertisement_marketing { get; set; }
        public bool is_construction { get; set; }
        public bool is_customer_service { get; set; }
        public bool is_design { get; set; }
        public bool is_engineering { get; set; }
        public bool is_facilities { get; set; }
        public bool is_finance { get; set; }
        public bool is_human_resources { get; set; }
        public bool is_it_and_development { get; set; }
        public bool is_legal { get; set; }
        public bool is_logistics { get; set; }
        public bool is_operation_and_production { get; set; }
        public bool is_real_estate { get; set; }
        public bool is_sales { get; set; }
    }

    public class UtilLeaveGridData
    {
        public string emp_id { get; set; }
        public string leave_status_name { get; set; }
        public string leave_name { get; set; }
        public string leave_start_date { get; set; }
        public string leave_end_date { get; set; }
        public string leave_type_name { get; set; }
        public string month { get; set; }
        public string days { get; set; }
    }

    //internal sealed class MyComparer : IEqualityComparer<T>
    //{
    //    public bool Equals(T x, Y y)
    //    {
    //        if (x == null)
    //            return y == null;
    //        else if (y == null)
    //            return false;
    //        else
    //            return x.id == y.id && x.full_name == y.full_name;
    //    }

    //    public int GetHashCode(T obj)
    //    {
    //        return obj.id.GetHashCode();
    //    }
    //}

}

namespace TimeAPI.API
{
    public static class StaticUtils
    {
        public static DataTable ConvertToDatatable<T>(this IList<T> data)
        {
            PropertyDescriptorCollection props =
                TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new DataTable();
            for (int i = 0; i < props.Count; i++)
            {
                PropertyDescriptor prop = props[i];
                table.Columns.Add(prop.Name, prop.PropertyType);
            }
            object[] values = new object[props.Count];
            foreach (T item in data)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = props[i].GetValue(item);
                }
                table.Rows.Add(values);
            }
            return table;
        }
    }
}

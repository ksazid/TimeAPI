using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace TimeAPI.API.Models
{
    public class Utils
    {
        public string ID { get; set; }
    }

    public class UtilsGroupIDAndDate
    {
        public string ID { get; set; }
        public string Date { get; set; }

    }

    public class UtilsOrgID
    {
        public string OrgID { get; set; }
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

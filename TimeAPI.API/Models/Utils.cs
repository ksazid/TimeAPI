using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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

    public class UtilsOrgID
    {
        public string OrgID { get; set; }
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




}

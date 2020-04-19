using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using TimeAPI.API.Models.TimesheetViewModels;

namespace TimeAPI.API.Models.TimesheetBreakPostViewModels
{
    public class TimesheetBreakPostViewModel
    {
        public string id { get; set; }
        public string orgid { get; set; }
        public string groupid { get; set; }
        public List<string> team_member_empid { get; set; }
        public string break_in { get; set; }
        public string break_out { get; set; }
        public string createdby { get; set; }
        public TimesheetCurrentLocationViewModel TimesheetCurrentLocationViewModel { get; set; }
    }


    public class TimesheetBreakOutViewModel
    {
        public List<string> team_member_empid { get; set; }
        public string groupid { get; set; }
        public string break_out { get; set; }
        public string modifiedby { get; set; }
        public bool is_inrange { get; set; }
        public TimesheetCurrentLocationViewModel TimesheetCurrentLocationViewModel { get; set; }
    }
}

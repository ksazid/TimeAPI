using System;
using System.Collections.Generic;
using System.Text;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface ITimesheetRepository : IRepository<Timesheet, string>
    {
        void CheckOutByEmpID(Timesheet entity);
        Timesheet FindTimeSheetByEmpID(string empid, string groupid);
        string TotalHours(string Checkin, string Checkout);

    }
}

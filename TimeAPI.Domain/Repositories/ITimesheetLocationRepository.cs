using System;
using System.Collections.Generic;
using System.Text;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface ITimesheetLocationRepository : IRepository<TimesheetLocation, string>
    {
        //void CheckOutByEmpID(Timesheet entity);
        //Timesheet FindTimeSheetByEmpID(string empid, string groupid);
        void RemoveByGroupID(string GroupID);

    }
}

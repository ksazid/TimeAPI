﻿using System;
using System.Collections.Generic;
using System.Text;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface ILocationRepository : IRepository<Location, string>
    {
        //void CheckOutByEmpID(Timesheet entity);
        //Timesheet FindTimeSheetByEmpID(string empid, string groupid);
        //void RemoveByGroupID(string GroupID);

    }
}

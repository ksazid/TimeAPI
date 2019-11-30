﻿using System;
using System.Collections.Generic;
using System.Text;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface ITimesheetRepository : IRepository<Timesheet, string>
    {
        public Timesheet CheckOutByEmpID(string key);
    }
}
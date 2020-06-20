﻿using System;
using System.Collections.Generic;
using System.Text;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface IEmployeeScreenshotRepository : IRepository<EmployeeScreenshot, string>
    {
        public EmployeeScreenshot FindByProfileEmpiID(string key);
    }
}

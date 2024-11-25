﻿using System;
using System.Collections.Generic;
using System.Text;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface IEmployeeTypeRepository : IRepository<EmployeeType, string>
    {
        IEnumerable<EmployeeType> GetEmployeeTypeByOrgID(string OrgID);
    }
}

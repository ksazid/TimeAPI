using System;
using System.Collections.Generic;
using System.Text;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface ITimesheetAdministrativeRepository : IRepository<TimesheetAdministrative, string>
    {
        void RemoveByGroupID(string GroupID);
        dynamic GetTop10TimesheetAdminActivityOnGroupIDAndAdminID(string GroupID, string AdministrativeID);
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface IEntityContactRepository : IRepository<EntityContact, string>
    {
        //void CheckOutByEmpID(Timesheet entity);
        //Timesheet FindTimeSheetByEmpID(string empid, string groupid);
        //void RemoveByGroupID(string GroupID);
        void RemoveByEntityID(string EntityID);
    }
}

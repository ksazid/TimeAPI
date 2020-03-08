using System;
using System.Collections.Generic;
using System.Text;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface IAdministrativeRepository : IRepository<Administrative, string>
    {
        RootObject GetByOrgID(string OrgID);

        dynamic GetAdministrativeTaskByOrgID(string OrgID);
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface IOrganizationSetupRepository : IRepository<OrganizationSetup, string>
    {
        //Organization FindByOrgName(string full_name);
        void RemoveByOrgID(string key);
        OrganizationSetup FindByEnitiyID(string EntityID);
    }
}

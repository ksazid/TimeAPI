using System;
using System.Collections.Generic;
using System.Text;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface ISuperadminOrganizationRepository : IRepository<SuperadminOrganization, string>
    {
        void RemoveByOrgID(string key);
    }
}

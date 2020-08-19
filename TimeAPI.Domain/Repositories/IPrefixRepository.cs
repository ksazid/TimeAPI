using System;
using System.Collections.Generic;
using System.Text;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface IPrefixRepository : IRepository<Prefix, string>
    {
        public IEnumerable<Prefix> PrefixByOrgID(string OrgID);
    }
}

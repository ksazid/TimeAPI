using System;
using System.Collections.Generic;
using System.Text;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface IWarrantyRepository : IRepository<Warranty, string>
    {
        public IEnumerable<Warranty> WarrantyByOrgID(string OrgID);
    }
}

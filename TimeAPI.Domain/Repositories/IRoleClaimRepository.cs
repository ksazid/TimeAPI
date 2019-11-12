using System.Collections.Generic;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface IRoleClaimRepository : IRepository<RoleClaim, int>
    {
        IEnumerable<RoleClaim> FindByRoleId(string roleId);
    }
}

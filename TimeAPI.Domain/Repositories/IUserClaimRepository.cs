using System.Collections.Generic;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface IUserClaimRepository : IRepository<UserClaim, int>
    {
        IEnumerable<UserClaim> GetByUserId(string userId);
        IEnumerable<User> GetUsersForClaim(string claimType, string claimValue);
    }
}

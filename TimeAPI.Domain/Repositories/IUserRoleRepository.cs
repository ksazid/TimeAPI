using System.Collections.Generic;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface IUserRoleRepository
    {
        void Add(string UserId, string roleName);
        void Remove(string userId, string roleName);
        IEnumerable<string> GetRoleNamesByUserId(string userId);
        IEnumerable<User> GetUsersByRoleName(string roleName);
    }
}

using System.Collections.Generic;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface IUserLoginRepository : IRepository<UserLogin, UserLoginKey>
    {
        IEnumerable<UserLogin> FindByUserId(string userId);
    }
}

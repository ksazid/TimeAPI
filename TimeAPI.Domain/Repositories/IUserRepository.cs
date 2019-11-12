using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface IUserRepository : IRepository<User, string>
    {
        User FindByNormalizedUserName(string normalizedUserName);

        User FindByNormalizedEmail(string normalizedEmail);
    }
}

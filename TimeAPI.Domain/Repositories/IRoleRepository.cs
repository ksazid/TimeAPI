using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface IRoleRepository : IRepository<Role, string>
    {
        Role FindByName(string roleName);
    }
}

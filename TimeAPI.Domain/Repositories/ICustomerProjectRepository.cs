using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface ICustomerProjectRepository : IRepository<CustomerProject, string>
    {
        //Role FindByName(string roleName);
    }
}

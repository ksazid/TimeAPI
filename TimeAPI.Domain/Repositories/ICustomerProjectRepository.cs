using System.Collections.Generic;
using System.Threading.Tasks;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface ICustomerProjectRepository : IRepositoryAsync<CustomerProject, string>
    {
        Task<IEnumerable<Customer>> FindCustomerByOrgID(string key);
        Task<CustomerProject> FindByProjectID(string key);

    }
}

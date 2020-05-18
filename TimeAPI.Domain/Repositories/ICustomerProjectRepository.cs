using System.Collections.Generic;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface ICustomerProjectRepository : IRepository<CustomerProject, string>
    {
        IEnumerable<Customer> FindCustomerByOrgID(string key);
        public CustomerProject FindByProjectID(string key);
    }
}

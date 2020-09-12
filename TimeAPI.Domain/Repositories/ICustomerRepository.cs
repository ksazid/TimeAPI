using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;
using System.Threading.Tasks;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Model;

namespace TimeAPI.Domain.Repositories
{
    public interface ICustomerRepository : IRepositoryAsync<Customer, string>
    {
        Task<IEnumerable<dynamic>> FindCustomerByOrgID(string OrgID);
        Task<Customer> FindCustomerByProjectID(string ProjectID);
        Task<Customer> FindByCustomerByNameAndEmail(string Name, string Email);

    }
}

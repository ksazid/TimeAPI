using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Model;

namespace TimeAPI.Domain.Repositories
{
    public interface ICustomerRepository : IRepository<Customer, string>
    {
        IEnumerable<dynamic> FindCustomerByOrgID(string OrgID);
        Customer FindCustomerByProjectID(string ProjectID);

    }
}

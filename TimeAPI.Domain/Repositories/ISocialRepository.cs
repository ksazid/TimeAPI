using System;
using System.Collections.Generic;
using System.Text;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface ISocialRepository : IRepository<Social, string>
    {
        IEnumerable<Social> FindByEmpID(string empid);
    }
}

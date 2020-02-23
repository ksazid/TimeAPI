using System;
using System.Collections.Generic;
using System.Text;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface IDelegationsDelegateeRepository : IRepository<DelegationsDelegatee, string>
    {
        void RemoveByDelegator(string key);
        void RemoveByDelegateeID(string key);
    }
}

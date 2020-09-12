using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface IPriorityRepository : IRepositoryAsync<Priority, string>
    {
        Task <IEnumerable<Priority>> GetPriorityByOrgID(string OrgID);
    }
}

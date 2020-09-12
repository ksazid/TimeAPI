using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface IWeekdaysRepository : IRepositoryAsync<Weekdays, string>
    {
        Task RemoveByOrgID(string OrgID);
        Task<IEnumerable<Weekdays>> FindByOrgID(string OrgID);
    }
}

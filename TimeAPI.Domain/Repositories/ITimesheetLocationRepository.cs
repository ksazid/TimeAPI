using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface ITimesheetLocationRepository : IRepositoryAsync<TimesheetLocation, string>
    {
        Task RemoveByGroupID(string GroupID);
    }
}

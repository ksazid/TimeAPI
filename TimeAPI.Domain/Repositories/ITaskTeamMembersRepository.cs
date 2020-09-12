using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;
using System.Threading.Tasks;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Model;

namespace TimeAPI.Domain.Repositories
{
    public interface ITaskTeamMembersRepository : IRepositoryAsync<TaskTeamMember, string>
    { 
        Task RemoveByTaskID(string TaskID);
        Task<IEnumerable<TaskTeamMember>> FindByTaskID(string TaskID);
    }
}

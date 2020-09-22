using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface IEntityCallRepository : IRepositoryAsync<EntityCall, string>
    {
        Task<dynamic> EntityCallByEntityID(string EntityID);
    }
}

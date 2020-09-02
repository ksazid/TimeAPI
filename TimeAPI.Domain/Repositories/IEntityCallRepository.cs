using System;
using System.Collections.Generic;
using System.Text;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface IEntityCallRepository : IRepository<EntityCall, string>
    {
        dynamic EntityCallByEntityID(string EntityID);
    }
}

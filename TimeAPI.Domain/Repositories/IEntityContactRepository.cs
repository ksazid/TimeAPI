using System;
using System.Collections.Generic;
using System.Text;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface IEntityContactRepository : IRepository<EntityContact, string>
    {
        void RemoveByEntityID(string EntityID);
        EntityContact FindByEntityID(string EntityID);

    }
}

using System;
using System.Collections.Generic;
using System.Text;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface IEntityContactRepository : IRepository<EntityContact, string>
    {
        void RemoveByEntityID(string EntityID);
        void UpdateByEntityID(EntityContact entity);
        EntityContact FindByEntityID(string EntityID);
        IEnumerable<EntityContact> FindByEntityListID(string EntityID);
        dynamic FindByEntityContactOrgID(string OrgID);

    }
}

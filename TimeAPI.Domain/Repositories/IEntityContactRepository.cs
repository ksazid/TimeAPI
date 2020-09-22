using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface IEntityContactRepository : IRepositoryAsync<EntityContact, string>
    {
        Task RemoveByEntityID(string EntityID);
        Task UpdateByEntityID(EntityContact entity);
        Task<EntityContact> FindByEntityID(string EntityID);
        Task<IEnumerable<EntityContact>> FindByEntityListID(string EntityID);
        Task<dynamic> FindByEntityContactOrgID(string OrgID);
        Task<IEnumerable<EntityContact>> GetAllEntityContactByEntityIDAndCstID(string EntityID, string CstID);

    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface IEntityNotesRepository : IRepositoryAsync<EntityNotes, string>
    {
        Task<IEnumerable<EntityNotes>> EntityNotesByOrgID(string OrgID);
        Task<IEnumerable<EntityNotes>> EntityNotesByEntityID(string EntityID);
    }
}

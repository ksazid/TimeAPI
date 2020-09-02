using System;
using System.Collections.Generic;
using System.Text;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface IEntityNotesRepository : IRepository<EntityNotes, string>
    {
        IEnumerable<EntityNotes> EntityNotesByOrgID(string OrgID);
        IEnumerable<EntityNotes> EntityNotesByEntityID(string EntityID);
    }
}

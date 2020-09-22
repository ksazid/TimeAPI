using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class EntityNotesRepository : RepositoryBase, IEntityNotesRepository
    {
        public EntityNotesRepository(IDbTransaction transaction)
           : base(transaction)
        { }
        public void Add(EntityNotes entity)
        {
            entity.id = ExecuteScalar<string>(
                    sql: @"INSERT INTO dbo.entity_notes
                            (id, org_id, entity_id, title, notes, created_date, createdby)
                    VALUES (@id, @org_id, @entity_id, @title, @notes, @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public async Task< EntityNotes> Find(string key)
        {
            return await QuerySingleOrDefaultAsync<EntityNotes>(
                sql: "SELECT * FROM dbo.entity_notes WHERE id = @key and is_deleted = 0",
                param: new { key }
            );
        }

        public async Task< IEnumerable<EntityNotes>> All()
        {
            return await QueryAsync<EntityNotes>(
                sql: "SELECT * FROM dbo.entity_notes where is_deleted = 0"
            );
        }

        public async Task<IEnumerable<EntityNotes>> EntityNotesByOrgID(string key)
        {
            return await QueryAsync<EntityNotes>(
                sql: "SELECT * FROM dbo.entity_notes where is_deleted = 0 and org_id = @key",
                param: new { key }
            );
        }

        public async Task<IEnumerable<EntityNotes>> EntityNotesByEntityID(string key)
        {
            return await QueryAsync<EntityNotes>(
                sql: "SELECT * FROM dbo.entity_notes where is_deleted = 0 and entity_id = @key",
                param: new { key }
            );
        }

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.entity_notes
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }

        public void Update(EntityNotes entity)
        {
            Execute(
                sql: @"UPDATE dbo.entity_notes
                           SET 
                            entity_id = @entity_id, 
                            title = @title,
                            notes = @notes,
                            modified_date = @modified_date,
                            modifiedby = @modifiedby
                         WHERE id = @id",
                param: entity
            );
        }
    }
}
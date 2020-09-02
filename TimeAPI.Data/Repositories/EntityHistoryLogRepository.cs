using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class EntityHistoryLogRepository : RepositoryBase, IEntityHistoryLogRepository
    {
        public EntityHistoryLogRepository(IDbTransaction transaction)
           : base(transaction)
        { }

        public async void Add(EntityHistoryLog entity)
        {
            entity.id = await ExecuteScalarAsync<string>(
                    sql: @"INSERT INTO dbo.entity_history_log
                            (id, entity_id, event_ondate, event_time, event_type, event_desc, event_before, event_after, event_user_id, event_modified_by, created_date, createdby)
                    VALUES  (@id, @entity_id, @event_ondate, @event_time, @event_type, @event_desc, @event_before, @event_after, @event_user_id, @event_modified_by, @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                ).ConfigureAwait(false);
        }

        public async Task<EntityHistoryLog> Find(string key)
        {
            return await QuerySingleOrDefaultAsync<EntityHistoryLog>(
                sql: "SELECT * FROM dbo.entity_history_log WHERE id = @key and is_deleted = 0",
                param: new { key }
            );
        }

        public async Task<IEnumerable<EntityHistoryLog>> All()
        {
            return await QueryAsync<EntityHistoryLog>(
                sql: "SELECT * FROM dbo.entity_history_log where is_deleted = 0"
                );
        }

        public async Task<dynamic> EntityHistoryLogByEntityID(string key)
        {
            return await QueryAsync<dynamic>(
                sql: @"SELECT * FROM dbo.entity_history_log
                        WHERE is_deleted = 0 
                        AND entity_id =  @key",
                param: new { key }
            );
        }

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.entity_history_log
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }

        public async void Update(EntityHistoryLog entity)
        {
            await ExecuteAsync(
                 sql: @"UPDATE dbo.entity_history_log
                           SET 
                            entity_id = @entity_id, 
                            event_ondate = @event_ondate, 
                            event_time = @event_time, 
                            event_type = @event_type,
                            event_desc = @event_desc, 
                            event_before = @event_before, 
                            event_after = @event_after, 
                            event_user_id = @event_user_id, 
                            event_modified_by = @event_modified_by,
                            modified_date = @modified_date,
                            modifiedby = @modifiedby
                         WHERE id = @id",
                 param: entity
             );
        }

    }
}
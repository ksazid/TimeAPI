using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class EntityCallRepository : RepositoryBase, IEntityCallRepository
    {
        public EntityCallRepository(IDbTransaction transaction)
           : base(transaction)
        { }

        public void Add(EntityCall entity)
        {
            entity.id = ExecuteScalar<string>(
                    sql: @"INSERT INTO dbo.entity_call
                            (id, entity_id, contact_id, subject, call_purpose, is_current_call, is_completed_call, is_schedule_call, start_time, end_time, call_desc, call_result, host, created_date, createdby)
                    VALUES (@id, @entity_id, @contact_id, @subject, @call_purpose, @is_current_call, @is_completed_call, @is_schedule_call, @start_time, @end_time, @call_desc, @call_result, @host, @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public async Task< EntityCall> Find(string key)
        {
            return await QuerySingleOrDefaultAsync<EntityCall>(
                sql: "SELECT * FROM dbo.entity_call WHERE id = @key and is_deleted = 0",
                param: new { key }
            );
        }

        public async Task<IEnumerable<EntityCall>> All()
        {
            return await QueryAsync<EntityCall>(
                sql: "SELECT * FROM dbo.entity_call where is_deleted = 0"
            );
        }

        public async Task<dynamic> EntityCallByEntityID(string key)
        {
            return await QueryAsync<dynamic>(
                sql: @"SELECT * FROM dbo.entity_call
                        WHERE is_deleted = 0 
                        AND entity_id =  @key",
                param: new { key }
            );
        }

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.entity_call
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }

        public void Update(EntityCall entity)
        {
            Execute(
                sql: @"UPDATE dbo.entity_call
                           SET 
                            entity_id = @entity_id, 
                            contact_id = @contact_id, 
                            subject = @subject,
                            call_purpose = @call_purpose, 
                            is_current_call = @is_current_call, 
                            is_completed_call = @is_completed_call, 
                            is_schedule_call = @is_schedule_call, 
                            start_time = @start_time, 
                            end_time = @end_time, 
                            call_desc = @call_desc, 
                            call_result = @call_result,
                            host = @host,
                            modified_date = @modified_date,
                            modifiedby = @modifiedby
                         WHERE id = @id",
                param: entity
            );
        }
    }
}
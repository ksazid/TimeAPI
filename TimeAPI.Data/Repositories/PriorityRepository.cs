using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class PriorityRepository : RepositoryBase, IPriorityRepository
    {
        public PriorityRepository(IDbTransaction transaction)
           : base(transaction)
        { }

        public void Add(Priority entity)
        {
            entity.id = ExecuteScalar<string>(
                    sql: @"
                    INSERT INTO dbo.priority
                            (id, org_id, priority_name, priority_desc, created_date, createdby)
                    VALUES (@id, @org_id, @priority_name, @priority_desc, @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public async Task<Priority> Find(string key)
        {
            return await QuerySingleOrDefaultAsync<Priority>(
                sql: "SELECT * FROM [dbo].[priority] WHERE id = @key and is_deleted = 0",
                param: new { key }
            );
        }

        public async Task<IEnumerable<Priority>> GetPriorityByOrgID(string key)
        {
            return await QueryAsync<Priority>(
                sql: @"SELECT * FROM [dbo].[priority]
                        WHERE org_id='default' AND is_deleted = 0
                        UNION 
                        SELECT * FROM [dbo].[priority]
                        WHERE org_id= @key AND is_deleted = 0  
                        ORDER BY priority_name ASC",
                param: new { key }
            );
        }

        public async Task<IEnumerable<Priority>> All()
        {
            return await QueryAsync<Priority>(
                sql: "SELECT * FROM [dbo].[priority] where is_deleted = 0"
            );
        }

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.priority
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }

        public void Update(Priority entity)
        {
            Execute(
                sql: @"UPDATE dbo.priority
                           SET org_id = @org_id, priority_name = @priority_name, priority_desc = @priority_desc, modified_date = @modified_date, modifiedby = @modifiedby
                         WHERE id = @id",
                param: entity
            );
        }
    }
}
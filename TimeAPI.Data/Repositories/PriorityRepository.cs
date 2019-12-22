using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
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

        public Priority Find(string key)
        {
            return QuerySingleOrDefault<Priority>(
                sql: "SELECT * FROM [dbo].[priority] WHERE id = @key and is_deleted = 0",
                param: new { key }
            );
        }

        public IEnumerable<Priority> All()
        {
            return Query<Priority>(
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

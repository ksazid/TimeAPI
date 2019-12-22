using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class StatusRepository : RepositoryBase, IStatusRepository
    {

        public StatusRepository(IDbTransaction transaction) : base(transaction)
        { }

        public void Add(Status entity)
        {

            entity.id = ExecuteScalar<string>(
                    sql: @"INSERT INTO dbo.status
                                  (id, org_id, status_name, status_desc, created_date, createdby)
                           VALUES (@id, @org_id, @status_name, @status_desc, @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public Status Find(string key)
        {
            return QuerySingleOrDefault<Status>(
                sql: "SELECT * FROM dbo.status WHERE is_deleted = 0 and id = @key",
                param: new { key }
            );
        }

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.status
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }

        public void Update(Status entity)
        {
            Execute(
                sql: @"UPDATE dbo.status
                   SET 
                    org_id = @org_id,
                    status_name = @status_name, 
                    status_desc = @status_desc, 
                    modified_date = @modified_date, 
                    modifiedby = @modifiedby
                    WHERE id = @id",
                param: entity
            );
        }

        public IEnumerable<Status> All()
        {
            return Query<Status>(
                sql: "SELECT * FROM [dbo].[status] where is_deleted = 0"
            );
        }

    }
}

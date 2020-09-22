using System.Collections.Generic;
using System.Data;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class LocalActivityRepository : RepositoryBase, ILocalActivityRepository
    {
        public LocalActivityRepository(IDbTransaction transaction)
           : base(transaction)
        { }

        public void Add(LocalActivity entity)
        {
            entity.id = ExecuteScalar<string>(
                    sql: @"INSERT INTO dbo.local_activity
                            (id, org_id, activity_name, activity_desc, created_date, createdby)
                    VALUES (@id, @org_id, @activity_name, @activity_desc, @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public LocalActivity Find(string key)
        {
            return QuerySingleOrDefault<LocalActivity>(
                sql: "SELECT * FROM dbo.local_activity WHERE id = @key and is_deleted = 0",
                param: new { key }
            );
        }

        public IEnumerable<LocalActivity> All()
        {
            return Query<LocalActivity>(
                sql: "SELECT * FROM dbo.local_activity where is_deleted = 0"
            );
        }

        public IEnumerable<LocalActivity> LocalActivityByOrgID(string key)
        {
            return Query<LocalActivity>(
                sql: "SELECT * FROM dbo.local_activity where is_deleted = 0 and org_id = @key",
                param: new { key }
            );
        }

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.local_activity
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }

        public void Update(LocalActivity entity)
        {
            Execute(
                sql: @"UPDATE dbo.local_activity
                           SET 
                            activity_name = @activity_name, 
                            activity_desc = @activity_desc,
                            modified_date = @modified_date,
                            modifiedby = @modifiedby
                         WHERE id = @id",
                param: entity
            );
        }
    }
}
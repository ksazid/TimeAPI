using System.Collections.Generic;
using System.Data;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class TimeoffTypeRepository : RepositoryBase, ITimeoffTypeRepository
    {
        public TimeoffTypeRepository(IDbTransaction transaction)
           : base(transaction)
        { }
        public void Add(TimeOff_Setup entity)
        {
            entity.id = ExecuteScalar<string>(
                    sql: @"INSERT INTO dbo.timeoff_type
                            (id, org_id, timeoff_type_name, timeoff_type_earned, created_date, createdby)
                    VALUES (@id, @org_id, @timeoff_type_name, @timeoff_type_earned, @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public TimeOff_Setup Find(string key)
        {
            return QuerySingleOrDefault<TimeOff_Setup>(
                sql: "SELECT * FROM dbo.timeoff_type WHERE id = @key and is_deleted = 0",
                param: new { key }
            );
        }

        public IEnumerable<TimeOff_Setup> FetchTimeoffTypeOrgID(string key)
        {
            return Query<TimeOff_Setup>(
                sql: "SELECT * FROM dbo.timeoff_type WHERE org_id = @key and is_deleted = 0",
                param: new { key }
            );
        }

        public IEnumerable<TimeOff_Setup> All()
        {
            return Query<TimeOff_Setup>(
                sql: "SELECT * FROM dbo.timeoff_type where is_deleted = 0"
            );
        }

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.timeoff_type
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }

        public void Update(TimeOff_Setup entity)
        {
            Execute(
                sql: @"UPDATE dbo.timeoff_type
                           SET 
                            org_id = @org_id, 
                            timeoff_type_name = @timeoff_type_name, 
                            timeoff_type_earned = @timeoff_type_earned,
                            modified_date = @modified_date,
                            modifiedby = @modifiedby
                         WHERE id = @id",
                param: entity
            );
        }
    }
}
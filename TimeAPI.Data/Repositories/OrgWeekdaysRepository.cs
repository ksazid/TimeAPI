using System.Collections.Generic;
using System.Data;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class OrgWeekdaysRepository : RepositoryBase, IOrgWeekdaysRepository
    {
        public OrgWeekdaysRepository(IDbTransaction transaction) : base(transaction)
        { }

        public void Add(OrgWeekdays entity)
        {
            entity.id = ExecuteScalar<string>(
                    sql: @"INSERT INTO dbo.org_x_weekdays
                                  (id, org_id, weekdays_id, created_date, createdby)
                           VALUES (@id, @org_id, @weekdays_id, @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public OrgWeekdays Find(string key)
        {
            return QuerySingleOrDefault<OrgWeekdays>(
                sql: "SELECT * FROM dbo.org_x_weekdays WHERE is_deleted = 0 and entity_id = @key",
                param: new { key }
            );
        }

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.org_x_weekdays
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE Find = @key",
                param: new { key }
            );
        }

        public void RemoveByEntityID(string key)
        {
            Execute(
                sql: @"UPDATE dbo.org_x_weekdays
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE entity_id = @entity_id",
                param: new { key }
            );
        }

        public void Update(OrgWeekdays entity)
        {
            Execute(
                sql: @"UPDATE dbo.org_x_weekdays
                   SET
                    org_id = @org_id, 
                    weekdays_id = @weekdays_id,
                    modified_date = @modified_date,
                    modifiedby = @modifiedby
                    WHERE entity_id = @entity_id",
                param: entity
            );
        }

        public IEnumerable<OrgWeekdays> All()
        {
            return Query<OrgWeekdays>(
                sql: "SELECT * FROM dbo.org_x_weekdays where is_deleted = 0"
            );
        }
    }
}
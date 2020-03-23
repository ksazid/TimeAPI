using System.Collections.Generic;
using System.Data;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class WeekdaysRepository : RepositoryBase, IWeekdaysRepository
    {
        public WeekdaysRepository(IDbTransaction transaction) : base(transaction)
        { }
        public void Add(Weekdays entity)
        {
            entity.id = ExecuteScalar<string>(
                    sql: @"INSERT INTO dbo.weekdays
                                  (id, org_id, day_name, from_time, to_time, is_off, created_date, createdby)
                           VALUES (@id, @org_id, @day_name, @from_time, @to_time, @is_off, @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public Weekdays Find(string key)
        {
            return QuerySingleOrDefault<Weekdays>(
                sql: "SELECT * FROM dbo.weekdays WHERE is_deleted = 0 and id = @key",
                param: new { key }
            );
        }

        public IEnumerable<Weekdays> FindByOrgID(string key)
        {
            return Query<Weekdays>(
                sql: "SELECT * FROM dbo.weekdays WHERE is_deleted = 0 and org_id = @key",
                param: new { key }
            );
        }

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.weekdays
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }

        public void RemoveByOrgID(string key)
        {
            Execute(
                sql: @"UPDATE dbo.weekdays
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE org_id = @key",
                param: new { key }
            );
        }

        public void Update(Weekdays entity)
        {
            Execute(
                sql: @"UPDATE dbo.weekdays
                   SET
                   day_name = @day_name,
                    from_time = @from_time,
                    to_time = @to_time, 
                    is_off =  @is_off,
                    modified_date = @modified_date,
                    modifiedby = @modifiedby
                    WHERE org_id = @org_id",
                param: entity
            );
        }

        public IEnumerable<Weekdays> All()
        {
            return Query<Weekdays>(
                sql: "SELECT * FROM dbo.weekdays where is_deleted = 0"
            );
        }
    }
}
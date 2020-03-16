using System.Collections.Generic;
using System.Data;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class WeekendHoursRepository : RepositoryBase, IWeekendHoursRepository
    {
        public WeekendHoursRepository(IDbTransaction transaction) : base(transaction)
        { }
        public void Add(WeekendHours entity)
        {
            entity.id = ExecuteScalar<string>(
                    sql: @"INSERT INTO dbo.weekend_hours
                                  (id, org_id, offworkdays, start_time, end_time, created_date, createdby)
                           VALUES (@id, @org_id, @offworkdays, @start_time, @end_time, @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public WeekendHours Find(string key)
        {
            return QuerySingleOrDefault<WeekendHours>(
                sql: "SELECT * FROM dbo.weekend_hours WHERE is_deleted = 0 and id = @key",
                param: new { key }
            );
        }

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.weekend_hours
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }

        public void RemoveByOrgID(string key)
        {
            Execute(
                sql: @"UPDATE dbo.weekend_hours
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE org_id = @org_id",
                param: new { key }
            );
        }

        public void Update(WeekendHours entity)
        {
            Execute(
                sql: @"UPDATE dbo.weekend_hours
                   SET
                    offworkdays = @offworkdays, 
                    start_time = @start_time, 
                    end_time = @end_time,
                    modified_date = @modified_date,
                    modifiedby = @modifiedby
                    WHERE org_id = @org_id",
                param: entity
            );
        }

        public IEnumerable<WeekendHours> All()
        {
            return Query<WeekendHours>(
                sql: "SELECT * FROM dbo.weekend_hours where is_deleted = 0"
            );
        }
    }
}
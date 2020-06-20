using System.Collections.Generic;
using System.Data;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class CostPerHourRepository : RepositoryBase, ICostPerHourRepository
    {
        public CostPerHourRepository(IDbTransaction transaction)
           : base(transaction)
        { }
        public void Add(CostPerHour entity)
        {
            entity.id = ExecuteScalar<string>(
                    sql: @"INSERT INTO dbo.cost_per_hour
                            (id, org_id, cost_per_hour,  created_date, createdby)
                    VALUES (@id, @org_id, @cost_per_hour, @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public CostPerHour Find(string key)
        {
            return QuerySingleOrDefault<CostPerHour>(
                sql: "SELECT * FROM dbo.cost_per_hour WHERE id = @key and is_deleted = 0",
                param: new { key }
            );
        }

        public IEnumerable<CostPerHour> FetchCostPerHourOrgID(string key)
        {
            return Query<CostPerHour>(
                sql: "SELECT * FROM dbo.cost_per_hour WHERE org_id = @key and is_deleted = 0",
                param: new { key }
            );
        }

        public IEnumerable<CostPerHour> All()
        {
            return Query<CostPerHour>(
                sql: "SELECT * FROM dbo.cost_per_hour where is_deleted = 0"
            );
        }

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.cost_per_hour
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }
        public void Update(CostPerHour entity)
        {
            Execute(
                sql: @"UPDATE dbo.cost_per_hour
                           SET 
                            org_id = @org_id, 
                            cost_per_hour = @cost_per_hour,
                            modified_date = @modified_date,
                            modifiedby = @modifiedby
                         WHERE id = @id",
                param: entity
            );
        }
    }
}
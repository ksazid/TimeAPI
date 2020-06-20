using System.Collections.Generic;
using System.Data;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class ProfitMarginRepository : RepositoryBase, IProfitMarginRepository
    {
        public ProfitMarginRepository(IDbTransaction transaction)
           : base(transaction)
        { }
        public void Add(ProfitMargin entity)
        {
            entity.id = ExecuteScalar<string>(
                    sql: @"INSERT INTO dbo.profit_margin
                            (id, org_id, profit_margin,  created_date, createdby)
                    VALUES (@id, @org_id, @profit_margin, @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public ProfitMargin Find(string key)
        {
            return QuerySingleOrDefault<ProfitMargin>(
                sql: "SELECT * FROM dbo.profit_margin WHERE id = @key and is_deleted = 0",
                param: new { key }
            );
        }

        public IEnumerable<ProfitMargin> FetchProfitMarginOrgID(string key)
        {
            return Query<ProfitMargin>(
                sql: "SELECT * FROM dbo.profit_margin WHERE org_id = @key and is_deleted = 0",
                param: new { key }
            );
        }

        public IEnumerable<ProfitMargin> All()
        {
            return Query<ProfitMargin>(
                sql: "SELECT * FROM dbo.profit_margin where is_deleted = 0"
            );
        }

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.profit_margin
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }
        public void Update(ProfitMargin entity)
        {
            Execute(
                sql: @"UPDATE dbo.profit_margin
                           SET 
                            org_id = @org_id, 
                            profit_margin = @profit_margin,
                            modified_date = @modified_date,
                            modifiedby = @modifiedby
                         WHERE id = @id",
                param: entity
            );
        }
    }
}
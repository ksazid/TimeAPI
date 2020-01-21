using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Model;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class PlanPriceRepository : RepositoryBase, IPlanPriceRepository
    {

        public PlanPriceRepository(IDbTransaction transaction) : base(transaction)
        { }

        public void Add(PlanPrice entity)
        {
            entity.id = ExecuteScalar<string>(
                    sql: @"INSERT INTO dbo.saas_plan_price
                                  (id, plan_id, price_amount, unit, created_date, createdby)
                           VALUES (@id, @plan_id, @price_amount, @unit, @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public PlanPrice Find(string key)
        {
            return QuerySingleOrDefault<PlanPrice>(
                sql: "SELECT * FROM dbo.saas_plan_price WHERE is_deleted = 0 and id = @key",
                param: new { key }
            );
        }

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.saas_plan_price
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }

        public void Update(PlanPrice entity)
        {
            Execute(
                sql: @"UPDATE dbo.saas_plan_price
                   SET 
                    plan_id = @plan_id, 
                    price_amount = @, 
                    unit = @unit,
                    modified_date = @modified_date, 
                    modifiedby = @modifiedby,
                    is_approver = @is_approver
                    WHERE id =  @id",
                param: entity
            );
        }

        public IEnumerable<PlanPrice> All()
        {
            return Query<PlanPrice>(
                sql: "SELECT * FROM dbo.saas_plan_price where is_deleted = 0"
            );
        }
    }
}

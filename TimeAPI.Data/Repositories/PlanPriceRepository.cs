using System.Collections.Generic;
using System.Data;
using TimeAPI.Domain.Entities;
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
                                  (id, plan_id, price_amount, billing_cycle, created_date, createdby)
                           VALUES (@id, @plan_id, @price_amount, @billing_cycle, @created_date, @createdby);
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
                    price_amount = @price_amount,
                    billing_cycle = @billing_cycle,
                    modified_date = @modified_date,
                    modifiedby = @modifiedby
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

        public dynamic GetAllPlanPrice()
        {
            return Query<dynamic>(
                   sql: @"SELECT
		                    dbo.saas_plan_price.id,
		                    dbo.saas_plan.plan_name,
		                    dbo.saas_plan_price.price_amount,
		                    dbo.saas_plan_price.billing_cycle,
		                    dbo.saas_plan_price.is_active
                      FROM dbo.saas_plan_price WITH(NOLOCK)
                      INNER JOIN dbo.saas_plan on dbo.saas_plan_price.plan_id = dbo.saas_plan.id
                      WHERE dbo.saas_plan_price.is_deleted = 0"
               );
        }
    }
}
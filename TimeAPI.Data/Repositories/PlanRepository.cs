using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Model;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class PlanRepository : RepositoryBase, IPlanRepository
    {

        public PlanRepository(IDbTransaction transaction) : base(transaction)
        { }

        public void Add(Plan entity)
        {
            entity.id = ExecuteScalar<string>(
                    sql: @"INSERT INTO dbo.saas_plan
                                  (id, plan_name, plan_desc, created_date, createdby)
                           VALUES (@id, @plan_name, @plan_desc, @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public Plan Find(string key)
        {
            return QuerySingleOrDefault<Plan>(
                sql: "SELECT * FROM dbo.saas_plan WHERE is_deleted = 0 and id = @key",
                param: new { key }
            );
        }

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.saas_plan
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }

        public void Update(Plan entity)
        {
            Execute(
                sql: @"UPDATE dbo.saas_plan
                   SET 
                    plan_name = @plan_name, 
                    plan_desc = @plan_desc,
                    modified_date = @modified_date, 
                    modifiedby = @modifiedby
                    WHERE id =  @id",
                param: entity
            );
        }

        public IEnumerable<Plan> All()
        {
            return Query<Plan>(
                sql: "SELECT * FROM dbo.saas_plan where is_deleted = 0"
            );
        }
    }
}

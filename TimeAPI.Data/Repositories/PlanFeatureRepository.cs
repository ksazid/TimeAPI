using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Model;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class PlanFeatureRepository : RepositoryBase, IPlanFeatureRepository
    {
        public PlanFeatureRepository(IDbTransaction transaction) : base(transaction)
        { }

        public void Add(PlanFeature entity)
        {
            entity.id = ExecuteScalar<string>(
                    sql: @"INSERT INTO dbo.saas_plan_features
                                  (id, plan_id, feature_name, feature_desc, created_date, createdby)
                           VALUES (@id, @plan_id, @feature_name, @feature_desc, @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public PlanFeature Find(string key)
        {
            return QuerySingleOrDefault<PlanFeature>(
                sql: "SELECT * FROM dbo.saas_plan_features WHERE is_deleted = 0 and id = @key",
                param: new { key }
            );
        }

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.saas_plan_features
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }

        public void Update(PlanFeature entity)
        {
            Execute(
                sql: @"UPDATE dbo.saas_plan_features
                   SET 
                    plan_id = @plan_id, 
                    feature_name = @feature_name,  
                    feature_desc =@feature_desc,
                    modified_date = @modified_date, 
                    modifiedby = @modifiedby
                    WHERE id =  @id",
                param: entity
            );
        }

        public IEnumerable<PlanFeature> All()
        {
            return Query<PlanFeature>(
                sql: "SELECT * FROM dbo.saas_plan_features where is_deleted = 0"
            );
        }
    }
}

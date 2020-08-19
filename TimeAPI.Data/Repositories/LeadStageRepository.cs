using System.Collections.Generic;
using System.Data;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class LeadStageRepository : RepositoryBase, ILeadStageRepository
    {
        public LeadStageRepository(IDbTransaction transaction) : base(transaction)
        { }

        public void Add(LeadStage entity)
        {
            entity.id = ExecuteScalar<string>(
                    sql: @"INSERT INTO dbo.lead_stage
                                  (id, org_id, stage_name, stage_desc, created_date, createdby)
                           VALUES (@id, @org_id, @stage_name, @stage_desc, @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public LeadStage Find(string key)
        {
            return QuerySingleOrDefault<LeadStage>(
                sql: "SELECT * FROM dbo.lead_stage WHERE is_deleted = 0 and id = @key",
                param: new { key }
            );
        }

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.lead_stage
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }

        public void Update(LeadStage entity)
        {
            Execute(
                sql: @"UPDATE dbo.lead_stage
                   SET
                    org_id = @org_id,
                    stage_name = @stage_name,
                    stage_desc = @stage_desc,
                    modified_date = @modified_date,
                    modifiedby = @modifiedby
                    WHERE id = @id",
                param: entity
            );
        }

        public IEnumerable<LeadStage> All()
        {
            return Query<LeadStage>(
                sql: "SELECT * FROM [dbo].[lead_stage] where is_deleted = 0"
            );
        }

        public IEnumerable<LeadStage> GetLeadStageByOrgID(string key)
        {
            return Query<LeadStage>(
                sql: @"SELECT * FROM [dbo].[lead_stage]
                        WHERE is_deleted = 0 and org_id = @key",
                param: new { key }
            );
        }
    }
}
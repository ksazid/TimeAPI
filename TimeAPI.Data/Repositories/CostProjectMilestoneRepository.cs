using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class CostProjectMilestoneRepository : RepositoryBase, ICostProjectMilestoneRepository
    {
        public CostProjectMilestoneRepository(IDbTransaction transaction) : base(transaction)
        { }

        public void Add(CostProjectMilestone entity)
        {
            entity.id = ExecuteScalar<string>(
                    sql: @"INSERT INTO dbo.cost_project_milestone
                                  (id, org_id, project_id, milestone_name, alias_name, created_date, createdby)
                           VALUES (@id, @org_id, @project_id, @milestone_name, @alias_name, @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public async Task<CostProjectMilestone> Find(string key)
        {
            return await QuerySingleOrDefaultAsync<CostProjectMilestone>(
                sql: "SELECT * FROM dbo.cost_project_milestone WHERE is_deleted = 0 and id = @key",
                param: new { key }
            );
        }

        public async Task<dynamic> FindByCostProjectMilestoneID(string key)
        {
            return await QueryAsync<dynamic>(
                   sql: @"SELECT 
                                dbo.cost_project_milestone.id as cost_project_milestone_id,
                                dbo.cost_project_milestone.project_id,
                                dbo.cost_project_milestone.milestone_name,
                                FROM dbo.cost_project_milestone 
                                WHERE dbo.cost_project_milestone.is_deleted = 0 and dbo.cost_project_milestone.id = @key",
                      param: new { key }
               );
        }

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.cost_project_milestone
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }

        public async Task RemoveByProjectID(string key)
        {
            await ExecuteAsync(
                sql: @"UPDATE dbo.cost_project_milestone
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE project_id = @key",
                param: new { key }
            );
        }

        public void Update(CostProjectMilestone entity)
        {
            Execute(
                sql: @"UPDATE dbo.cost_project_milestone
                   SET
                    org_id = @org_id,
                    project_id = @project_id,
                    milestone_name = @milestone_name,
                    alias_name = @alias_name,
                    modified_date = @modified_date,
                    modifiedby = @modifiedby
                    WHERE id = @id",
                param: entity
            );
        }

        public async Task<IEnumerable<CostProjectMilestone>> All()
        {
            return await QueryAsync<CostProjectMilestone>(
                sql: "SELECT * FROM [dbo].[cost_project_milestone] where is_deleted = 0"
            );
        }

        public async Task<IEnumerable<CostProjectMilestone>> GetCostProjectMilestoneByProjectID(string key)
        {
            return await QueryAsync<CostProjectMilestone>(
                sql: @"SELECT dbo.cost_project_milestone.*
                        FROM dbo.cost_project_milestone 
                    WHERE dbo.cost_project_milestone.is_deleted = 0 
                    and dbo.cost_project_milestone.project_id = @key",
                param: new { key }
            );
        }

        public async Task<IEnumerable<CostProjectMilestone>> GetAllStaticMilestoneByOrgID(string OrgID)
        {
            return await QueryAsync<CostProjectMilestone>(
                sql: "SELECT * FROM [dbo].[static_milestone] where is_deleted = 0 and org_id = @OrgID",
                 param: new { OrgID }
            );
        }
    }
}
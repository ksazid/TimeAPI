using System.Collections.Generic;
using System.Data;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class CostProjectMilestoneRepository : RepositoryBase, ICostProjectMilestoneRepository
    {
        public CostProjectMilestoneRepository(IDbTransaction transaction) : base(transaction)
        { }

        public void Add(CostProjectMilestone  entity)
        {
            entity.id = ExecuteScalar<string>(
                    sql: @"INSERT INTO dbo.cost_project_milestone
                                  (id, org_id, project_id, milestone_name, created_date, createdby)
                           VALUES (@id, @org_id, @project_id, @milestone_name, @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public CostProjectMilestone  Find(string key)
        {
            return QuerySingleOrDefault<CostProjectMilestone >(
                sql: "SELECT * FROM dbo.cost_project_milestone WHERE is_deleted = 0 and id = @key",
                param: new { key }
            );
        }

        public dynamic FindByCostProjectMilestoneID(string key)
        {
            return Query<dynamic>(
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

        public void RemoveByProjectID(string key)
        {
            Execute(
                sql: @"UPDATE dbo.cost_project_milestone
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE project_id = @key",
                param: new { key }
            );
        }
        
        public void Update(CostProjectMilestone  entity)
        {
            Execute(
                sql: @"UPDATE dbo.cost_project_milestone
                   SET
                    org_id = @org_id,
                    project_id = @project_id,
                    milestone_name = @milestone_name,
                    modified_date = @modified_date,
                    modifiedby = @modifiedby
                    WHERE id = @id",
                param: entity
            );
        }

        public IEnumerable<CostProjectMilestone > All()
        {
            return Query<CostProjectMilestone >(
                sql: "SELECT * FROM [dbo].[cost_project_milestone] where is_deleted = 0"
            );
        }

        public IEnumerable<CostProjectMilestone > GetCostProjectMilestoneByProjectID(string key)
        {
            return Query<CostProjectMilestone >(
                sql: @"SELECT dbo.cost_project_milestone.*
                        FROM dbo.cost_project_milestone 
                    WHERE dbo.cost_project_milestone.is_deleted = 0 
                    and dbo.cost_project_milestone.project_id = @key",
                param: new { key }
            );
        }

        public IEnumerable<CostProjectMilestone> GetAllStaticMilestoneByOrgID(string OrgID)
        {
            return Query<CostProjectMilestone>(
                sql: "SELECT * FROM [dbo].[static_milestone] where is_deleted = 0"
            );
        }
        

        //public void UpdateCostProjectMilestoneStatusByActivityID(CostProjectMilestone  entity)
        //{
        //    Execute(
        //        sql: @"UPDATE dbo.cost_project_milestone
        //           SET
        //            status_id = @status_id,
        //            modified_date = @modified_date,
        //            modifiedby = @modifiedby
        //            WHERE project_id = @project_id",
        //        param: entity
        //    );
        //}

        //public dynamic GetCostProjectMilestoneRatioByProjectID(string key)
        //{
        //    return Query<dynamic>(
        //           sql: @"SELECT 
        //                   dbo.project_status.project_status_name, 
        //                   count(*) * 100 / sum(count(*))  over() as ratio
        //            FROM dbo.cost_project_milestone WITH(NOLOCK)
        //                INNER JOIN dbo.project_status on dbo.cost_project_milestone.status_id = dbo.project_status.id
        //            WHERE dbo.cost_project_milestone.project_id = @key
        //                AND dbo.cost_project_milestone.is_deleted = 0 
        //                group by dbo.project_status.project_status_name",
        //              param: new { key }
        //       );
        //}
    }
}
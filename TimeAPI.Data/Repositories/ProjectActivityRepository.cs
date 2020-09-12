using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class ProjectActivityRepository : RepositoryBase, IProjectActivityRepository
    {
        public ProjectActivityRepository(IDbTransaction transaction) : base(transaction)
        { }

        public void Add(ProjectActivity entity)
        {
            entity.id = ExecuteScalar<string>(
                    sql: @"INSERT INTO dbo.project_activity
                                  (id, project_id, activity_name, activity_desc, unit, qty, start_date, end_date, is_approve_req, approved_id, is_approved, status_id, created_date, createdby)
                           VALUES (@id, @project_id, @activity_name, @activity_desc, @unit, @qty, @start_date, @end_date, @is_approve_req, @approved_id, @is_approved, @status_id, @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public async Task<ProjectActivity> Find(string key)
        {
            return await QuerySingleOrDefaultAsync<ProjectActivity>(
                sql: "SELECT * FROM dbo.project_activity WHERE is_deleted = 0 and id = @key",
                param: new { key }
            );
        }

        public async Task<dynamic> FindByProjectActivityID(string key)
        {
            return await QueryAsync<dynamic>(
                   sql: @"SELECT 
                                dbo.project_activity.id as project_activity_id,
                                dbo.project_activity.project_id,
                                dbo.project_activity.activity_name,
                                dbo.project_activity.activity_desc,
                                dbo.project_activity.unit,
                                dbo.project_activity.qty,
                                dbo.project_activity.is_approve_req,
                                dbo.project_activity.approved_id,
                                dbo.project_activity.is_approved,
                                dbo.status.id as status_id,
                                dbo.status.status_name,
                                dbo.project_activity.start_date,
                                dbo.project_activity.end_date
                                FROM dbo.project_activity 
                                LEFT JOIN  dbo.status on dbo.project_activity.status_id = status.id
                                WHERE dbo.project_activity.is_deleted = 0 and dbo.project_activity.id = @key",
                      param: new { key }
               );
        }

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.project_activity
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }

        public void Update(ProjectActivity entity)
        {
            Execute(
                sql: @"UPDATE dbo.project_activity
                   SET
                    project_id = @project_id,
                    activity_name = @activity_name,
                    activity_desc = @activity_desc,
                    start_date  = @start_date,
                    end_date = @end_date,
                    unit = @unit,
                    qty = @qty,
                    is_approve_req = @is_approve_req,
                    approved_id = @approved_id,
                    status_id =@status_id,
                    modified_date = @modified_date,
                    modifiedby = @modifiedby
                    WHERE id = @id",
                param: entity
            );
        }

        public async Task<IEnumerable<ProjectActivity>> All()
        {
            return await QueryAsync<ProjectActivity>(
                sql: "SELECT * FROM [dbo].[project_activity] where is_deleted = 0"
            );
        }

        public async Task<IEnumerable<ProjectActivity>> GetProjectActivityByProjectID(string key)
        {
            return await QueryAsync<ProjectActivity>(
                sql: @"SELECT dbo.project_activity.*, status.status_name  FROM dbo.project_activity 
                        LEFT JOIN status on dbo.project_activity.status_id = status.id 
                    WHERE dbo.project_activity.is_deleted = 0 and dbo.project_activity.project_id = @key",
                param: new { key }
            );
        }

        public async Task UpdateProjectActivityStatusByActivityID(ProjectActivity entity)
        {
            await ExecuteAsync(
                  sql: @"UPDATE dbo.project_activity
                   SET
                    status_id = @status_id,
                    modified_date = @modified_date,
                    modifiedby = @modifiedby
                    WHERE project_id = @project_id",
                  param: entity
              );
        }

        public async Task<dynamic> GetProjectActivityRatioByProjectID(string key)
        {
            return await QueryAsync<dynamic>(
                   sql: @"SELECT 
                           dbo.project_status.project_status_name, 
                           count(*) * 100 / sum(count(*))  over() as ratio
                    FROM dbo.project_activity WITH(NOLOCK)
                        INNER JOIN dbo.project_status on dbo.project_activity.status_id = dbo.project_status.id
                    WHERE dbo.project_activity.project_id = @key
                        AND dbo.project_activity.is_deleted = 0 
                        group by dbo.project_status.project_status_name",
                      param: new { key }
               );
        }
    }
}
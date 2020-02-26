using System.Collections.Generic;
using System.Data;
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

        public ProjectActivity Find(string key)
        {
            return QuerySingleOrDefault<ProjectActivity>(
                sql: "SELECT * FROM dbo.project_activity WHERE is_deleted = 0 and id = @key",
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

        public IEnumerable<ProjectActivity> All()
        {
            return Query<ProjectActivity>(
                sql: "SELECT * FROM [dbo].[project_activity] where is_deleted = 0"
            );
        }

        public IEnumerable<ProjectActivity> GetProjectActivityByProjectID(string key)
        {
            return Query<ProjectActivity>(
                sql: "SELECT * FROM dbo.project_activity WHERE is_deleted = 0 and project_id = @key",
                param: new { key }
            );
        }

        public void UpdateProjectActivityStatusByActivityID(ProjectActivity entity)
        {
            Execute(
                sql: @"UPDATE dbo.project_activity
                   SET
                    status_id =@status_id,
                    modified_date = @modified_date,
                    modifiedby = @modifiedby
                    WHERE project_id = @project_id",
                param: entity
            );
        }

        public dynamic GetProjectActivityRatioByProjectID(string key)
        {
            return Query<dynamic>(
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